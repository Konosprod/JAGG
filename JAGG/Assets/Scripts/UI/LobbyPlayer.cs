using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;
using Steamworks;
using System.Collections;
using System.IO;
using SimpleJSON;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Ionic.Zip;
using System;
using Newtonsoft.Json.Linq;

public class LobbyPlayer : NetworkLobbyPlayer {

    public Text playerNameLabel;
    public Toggle toggleReady;
    public Text readyText;
    public Button kickButton;
    public Image avatar;

    private static Dictionary<ulong, Texture2D> avatarCache = new Dictionary<ulong, Texture2D>();

    public Color localPlayerColor = new Color(1, 1, 1);

    protected Callback<AvatarImageLoaded_t> avatarLoadedCallback;

    [SyncVar(hook = "OnMyName")]
    public string playerName = "";

    public LobbyControls lobbyControls = null;

    private AuthenticationManager authenticationManager;

    // Use this for initialization
    void Start () {
        LobbyPlayerList._instance.AddPlayer(this);
        //DontDestroyOnLoad(this);

        kickButton.onClick.AddListener(KickPlayer);

        if (isLocalPlayer)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupOtherPlayer();
        }

        authenticationManager = AuthenticationManager._instance;

        avatarLoadedCallback = Callback<AvatarImageLoaded_t>.Create(OnAvatarLoaded);
    }

    public void ResetStatus()
    {
        if (isLocalPlayer)
        {
            SendNotReadyToBeginMessage();
            CmdResetStatus();
        }
    }

    public override void OnClientEnterLobby()
    {
        if (lobbyControls == null)
            lobbyControls = GameObject.FindObjectOfType<LobbyControls>();

        if (lobbyControls.selectedScene != "")
            LobbyPlayerList._instance.UpdateSelectedMap(lobbyControls.selectedScene);

        OnMyName(playerName);
        OnClientReady(toggleReady.isOn);

        LobbyPlayerList._instance.UpdateAvatar(SteamUser.GetSteamID().m_SteamID);
    }

    private void SetupLocalPlayer()
    {
        toggleReady.onValueChanged.RemoveAllListeners();
        toggleReady.onValueChanged.AddListener(OnReadyClicked);

        if (playerName == "")CmdUpdateAvatar(SteamUser.GetSteamID().m_SteamID);
            CmdNameChanged(SteamFriends.GetPersonaName());

        if (isServer)
        {
            lobbyControls.EnableEditButton(isServer);
            lobbyControls.EnableRulesButton(isServer);
        }

        CmdUpdateAvatar(SteamUser.GetSteamID().m_SteamID);

        lobbyControls.lobbyPlayer = this;
    }

    public void UpdateSelectedScene(string value)
    {
        CmdUpdateSelectedScene(value);
    }

    private void SetupOtherPlayer()
    {
        toggleReady.interactable = false;

        if(isServer)
        {
            kickButton.gameObject.SetActive(true);
        }

        //playerName.text = "Player " + LobbyManager.instance.numPlayers.ToString();
    }

    private void OnMyName(string newName)
    {
        playerName = newName;
        playerNameLabel.text = playerName;

        if(isLocalPlayer)
        {
            playerNameLabel.color = localPlayerColor;
        }
    }

    private void OnReadyClicked(bool newValue)
    {
        string levelName = lobbyControls.lobbyLevelName.text;
        string levelId = "";

        if (levelName.IndexOf("_") > 0)
            levelId = levelName.Substring(0, levelName.IndexOf("_"));

        CheckLevel(levelId, newValue);
    }

    public override void OnClientReady(bool readyState)
    {
        if(readyState)
        {
            toggleReady.isOn = true;
            readyText.text = "Ready";
        }
        else
        {
            toggleReady.isOn = false;
            readyText.text = "Not Ready";
        }
    }

    private void CheckLevel(string levelId, bool isReady)
    {
        //Scene name doesn't contain "_" it's a local level
        if (levelId == "")
        {
            if (isReady)
                SendReadyToBeginMessage();
            else
                SendNotReadyToBeginMessage();
        }
        else
        {
            string path = Application.persistentDataPath + "/Levels/";
            string searchPattern = levelId + "_*";
            string[] fileInfos = Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);


            if (fileInfos.Length > 0)
            {
                ZipFile mapFile = new ZipFile(fileInfos[0]);

                //Don't forget to create the directory maybe
                string tmpPath = Path.Combine(Application.temporaryCachePath, Path.GetFileName(path));
                //string tmpPath = Path.Combine(Application.temporaryCachePath, "test");

                mapFile.ExtractAll(tmpPath, ExtractExistingFileAction.OverwriteSilently);

                long timestamp = UnixTime(mapFile["level.json"].CreationTime);

                mapFile.Dispose();

                StartCoroutine(CheckTimestamp(levelId, timestamp, isReady));

            }
            else
            {
                toggleReady.interactable = false;
                StartCoroutine(DownloadMap(levelId));
            }
        }
    }

    IEnumerator CheckTimestamp(string levelId, long localTime, bool isReady)
    {
        string url = "https://jagg-api.konosprod.fr/api/maps/" + levelId;
        UnityWebRequest uwr = UnityWebRequest.Get(url);

        yield return uwr.SendWebRequest();

        if(uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            JObject mapInfo = JObject.Parse(uwr.downloadHandler.text);
            DateTime dt = DateTime.Parse(mapInfo["last_update"].Value<string>());
            Debug.Log("Local time : " + localTime.ToString() + " Last update : " + UnixTime(dt));

            //If there is a difference of 10s between localtime and server time, we consider that we should download
            //the map, it must be a new one
            if(UnixTime(dt) - localTime > 10)
            {
                Debug.Log(UnixTime(dt) - localTime);
                toggleReady.interactable = false;
                StartCoroutine(DownloadMap(levelId));
            }
            else
            {
                if (isReady)
                    SendReadyToBeginMessage();
                else
                    SendNotReadyToBeginMessage();
            }

        }
    }

    IEnumerator DownloadMap(string levelId)
    {
        string url = "https://jagg-api.konosprod.fr/api/maps/" + levelId + "/download";
        lobbyControls.mapDownloading.gameObject.SetActive(true);
        lobbyControls.mapDownloading.PlayAnimation();

        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            uwr.SetRequestHeader("Cookie", authenticationManager.sessionCookie);
            uwr.SetRequestHeader("User-Agent", @"Mozilla / 5.0(Android 4.4; Mobile; rv: 41.0) Gecko / 41.0 Firefox / 41.0");

            AsyncOperation request = uwr.SendWebRequest();

            while (!request.isDone)
            {
                ulong totalSize;
                ulong.TryParse(uwr.GetResponseHeader("Content-Length"), out totalSize);
                lobbyControls.progressBar.value = request.progress * 100;
                lobbyControls.percentageText.text = lobbyControls.progressBar.value.ToString("0.##") + " %";
                yield return null;
            }


            if (uwr.isNetworkError || uwr.isHttpError)
            {
                lobbyControls.progressBar.value = 0;
                lobbyControls.mapDownloading.gameObject.SetActive(false);
                Debug.Log(uwr.error);
                Debug.Log(uwr.responseCode);
            }
            else
            {
                string path = Application.persistentDataPath + "/levels/" + lobbyControls.lobbyLevelName.text + ".map";


                System.IO.File.WriteAllBytes(path, uwr.downloadHandler.data);
                lobbyControls.mapDownloading.gameObject.SetActive(false);
                toggleReady.interactable = true;
                SendReadyToBeginMessage();
            }
        }
    }

    private void OnAvatarLoaded(AvatarImageLoaded_t callback)
    {
        ulong steamId = callback.m_steamID.m_SteamID;
        Texture2D texture = null;

        if (avatarCache.ContainsKey(steamId))
        {
            texture = avatarCache[steamId];
        }
        else
        {
            texture = GetSteamImageAsTexture2D(callback.m_iImage);
            avatarCache.Add(steamId, texture);
        }

        avatar.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }

    public string ByteArrayToString(byte[] ba)
    {
        string hex = System.BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }

    private Texture2D GetSteamImageAsTexture2D(int iImage)
    {
        Texture2D ret = null;
        uint ImageWidth;
        uint ImageHeight;
        bool bIsValid = SteamUtils.GetImageSize(iImage, out ImageWidth, out ImageHeight);

        if (bIsValid)
        {
            byte[] Image = new byte[ImageWidth * ImageHeight * 4];

            bIsValid = SteamUtils.GetImageRGBA(iImage, Image, (int)(ImageWidth * ImageHeight * 4));
            if (bIsValid)
            {
                ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                ret.LoadRawTextureData(Image);
                ret.Apply();

                ret = FlipTexture(ret);
            }
        }

        return ret;
    }

    private Texture2D FlipTexture(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;

        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                flipped.SetPixel(i, yN - j - 1, original.GetPixel(i, j));
            }
        }

        flipped.Apply();

        return flipped;
    }

    private void KickPlayer()
    {
        LobbyManager._instance.playerManager.RemovePlayer(connectionToClient.connectionId);
        connectionToClient.Disconnect();
    }

    public long UnixTime(DateTime time)
    {
        var timeSpan = (time - new DateTime(1970, 1, 1, 0, 0, 0));
        return (long)timeSpan.TotalSeconds;
    }

    #region Command

    [Command]
    public void CmdUpdateAvatar(ulong steamid)
    {
        RpcUpdateAvatar(steamid);
    }

    [Command]
    public void CmdNameChanged(string name)
    {
        playerName = name;
    }

    [Command]
    public void CmdResetStatus()
    {
        toggleReady.isOn = false;
        SendNotReadyToBeginMessage();
    }

    [Command]
    public void CmdUpdateSelectedScene(string value)
    {
        lobbyControls.labelLevelName.text = value;
        lobbyControls.lobbyLevelName.text = value;
        RpcUpdateSelectedScene(value);
    }

#endregion

    [ClientRpc]
    public void RpcUpdateSelectedScene(string value)
    {
        lobbyControls.labelLevelName.text = value;
        lobbyControls.lobbyLevelName.text = value;
        lobbyControls.UpdateMapPreview();

        LobbyManager._instance.customMapFile = value;
    }

    [ClientRpc]
    public void RpcUpdateAvatar(ulong steamid)
    {
        int avatarId = SteamFriends.GetLargeFriendAvatar(new CSteamID(steamid));

        if (avatarId != -1)
        {
            Texture2D texture = null;
            if (avatarCache.ContainsKey(steamid))
            {
                texture = avatarCache[steamid];
            }
            else
            {
                texture = GetSteamImageAsTexture2D(avatarId);
                avatarCache.Add(steamid, texture);
            }

            avatar.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        }
    }
}
