using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;
using Steamworks;
using System.Collections;
using System.IO;
using SimpleJSON;

public class LobbyPlayer : NetworkLobbyPlayer {

    public GameObject socle;
    public GameObject canvas;
    public GameObject ball;

    public Text playerNameLabel;
    public Toggle toggleReady;
    public Text readyText;

    [SyncVar/*(hook = "OnPosition")*/]
    public Vector3 position;

    public Color localPlayerColor = new Color(1, 1, 1);

    [SyncVar(hook = "OnMyName")]
    public string playerName = "";

    public LobbyControls lobbyControls;

    private string sessionCookie = "";
    private bool isAuthenticated = false;

    // Use this for initialization
    void Start () {
        LobbyPlayerList._instance.AddPlayer(this);
        DontDestroyOnLoad(this);

        lobbyControls = GameObject.FindObjectOfType<LobbyControls>();

        if (isLocalPlayer)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupOtherPlayer();
        }
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
        OnMyName(playerName);
        OnClientReady(toggleReady.isOn);
        OnPosition(position);
    }

    private void SetupLocalPlayer()
    {
        toggleReady.onValueChanged.RemoveAllListeners();
        toggleReady.onValueChanged.AddListener(OnReadyClicked);

        if (playerName == "")
            CmdNameChanged(SteamFriends.GetPersonaName());

        if (isServer)
        {
            lobbyControls.EnableEditButton(isServer);
            lobbyControls.EnableRulesButton(isServer);
        }

        lobbyControls.lobbyPlayer = this;
    }

    public void UpdateSelectedScene(string value)
    {
        CmdUpdateSelectedScene(value);
    }

    private void SetupOtherPlayer()
    {
        toggleReady.interactable = false;

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

    private void OnPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    private void OnReadyClicked(bool newValue)
    {
        string levelName = lobbyControls.lobbyLevelName.text;
        string levelId = "";

        if (levelName.IndexOf("_") > 0)
            levelId = levelName.Substring(0, levelName.IndexOf("_"));

        bool hasLevel = CheckLevel(levelId);

        if (hasLevel)
        {
            if (newValue == true)
            {
                SendReadyToBeginMessage();
            }
            else
            {
                SendNotReadyToBeginMessage();
            }
        }
        else
        {
            toggleReady.interactable = false;
            StartCoroutine(GetAuthentication(levelId));
        }
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

    public void SetVisibility(bool visi)
    {
        socle.SetActive(visi);
        canvas.SetActive(visi);
        ball.SetActive(visi);
    }

    private bool CheckLevel(string levelId)
    {
        string path = Application.persistentDataPath + "/Levels/";
        string searchPattern = levelId + "_*";
        string[] fileInfos = Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

        return (fileInfos.Length > 0);
    }

    IEnumerator GetAuthentication(string levelId)
    {

        UnityWebRequest request = UnityWebRequest.Get("https://jagg.konosprod.fr/api/auth");
        request.SetRequestHeader("Cookie", sessionCookie);
        request.SetRequestHeader("User-Agent", @"Mozilla / 5.0(Android 4.4; Mobile; rv: 41.0) Gecko / 41.0 Firefox / 41.0");

        yield return request.SendWebRequest();


        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log(request.error);
            Debug.Log(request.responseCode);
        }
        else
        {
            JSONNode node = JSON.Parse(request.downloadHandler.text);
            isAuthenticated = node["auth"].AsBool;

            if (!isAuthenticated)
            {
                Debug.Log("Not Authenticated");
                StartCoroutine(Authenticate(levelId));
            }
            else
            {
                StartCoroutine(DownloadMap(levelId));
            }
        }
    }

    IEnumerator Authenticate(string levelId)
    {
        byte[] ticket = new byte[1024];
        uint ticketSize = 0;
        SteamUser.GetAuthSessionTicket(ticket, 1000, out ticketSize);

        WWWForm form = new WWWForm();
        form.AddField("ticket", ByteArrayToString(ticket));
        form.AddField("steamid", (SteamUser.GetSteamID().m_SteamID).ToString());

        UnityWebRequest www = UnityWebRequest.Post("https://jagg.konosprod.fr/api/auth", form);
        www.SetRequestHeader("Cookie", sessionCookie);
        www.SetRequestHeader("User-Agent", @"Mozilla / 5.0(Android 4.4; Mobile; rv: 41.0) Gecko / 41.0 Firefox / 41.0");

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.responseCode);
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            JSONNode node = JSON.Parse(www.downloadHandler.text);

            if (node["auth"].AsBool)
            {
                Debug.Log("Authenticated");
                sessionCookie = www.GetResponseHeader("Set-Cookie");
                isAuthenticated = true;
                StartCoroutine(DownloadMap(levelId));
            }
            else
            {
                Debug.Log("error while authenticating");
                isAuthenticated = false;
            }
        }

    }

    IEnumerator DownloadMap(string levelId)
    {
        string url = "https://jagg.konosprod.fr/api/maps/" + levelId + "/download";
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            uwr.SetRequestHeader("Cookie", sessionCookie);
            uwr.SetRequestHeader("User-Agent", @"Mozilla / 5.0(Android 4.4; Mobile; rv: 41.0) Gecko / 41.0 Firefox / 41.0");

            AsyncOperation request = uwr.SendWebRequest();

            while (!request.isDone)
            {
                ulong totalSize;
                ulong.TryParse(uwr.GetResponseHeader("Content-Length"), out totalSize);
                Debug.Log(request.progress * 100);
                yield return null;
            }


            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
                Debug.Log(uwr.responseCode);
            }
            else
            {
                //progressBar.value = 0;
                string path = Application.persistentDataPath + "/levels/" + lobbyControls.lobbyLevelName.text + ".map";
                /*
                foreach(string s in uwr.GetResponseHeaders().Keys)
                {
                    Debug.Log(s + " : " + uwr.GetResponseHeader(s));
                }*/
                //Debug.Log(path);
                System.IO.File.WriteAllBytes(path, uwr.downloadHandler.data);
            }
        }
    }

    public string ByteArrayToString(byte[] ba)
    {
        string hex = System.BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }

    #region Command

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
}
