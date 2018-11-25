using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System;

public class LobbyControls : MonoBehaviour {

    public Transform contentPanel;
    public GameObject prefabButton;

    [Header("UI")]
    public Button editButton;
    public Button returnButton;
    public Button selectButton;
    public Button rulesButton;
    public LoadingOverlay mapDownloading;
    public Slider progressBar;
    public Text percentageText;

    [Header("List Scene")]
    public Text labelLevelName;
    public Image imageScenePreview;
    public Text lobbyLevelName;
    public GameObject panelListScene;
    public GameObject panelVersionInfo;
    public Text labelVersion;
    public Text labelAuthor;

    [Header("Game Logic")]
    public string selectedScene;

    //[SyncVar(hook ="OnSelectedSceneChange")]
    public string levelName;

    public CustomLevel levelInfo;

    public LobbyManager lobbyManager;

    public LobbyPlayer lobbyPlayer;

    void Start()
    {
        ListScenes();
    }

    void OnEnable()
    {
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(VersionCheck);

        returnButton.onClick.RemoveListener(OnReturnButtonClick);
        returnButton.onClick.AddListener(OnReturnButtonClick);
    }

    public void ListScenes()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for(int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);

            if(path.StartsWith("Assets/Scenes/Levels"))
            {
                GameObject newButton = Instantiate(prefabButton, contentPanel);

                SceneListEntry entry = newButton.GetComponent<SceneListEntry>();
                entry.SetUp(Path.GetFileNameWithoutExtension(path), this);
            }
        }

        if(!Directory.Exists(Path.Combine(Application.persistentDataPath, "levels")))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "levels"));
        }

        foreach(string file in Directory.GetFiles(Path.Combine(Application.persistentDataPath, "levels"), "*.map"))
        {
            GameObject newButton = Instantiate(prefabButton, contentPanel);

            SceneListEntry entry = newButton.GetComponent<SceneListEntry>();
            entry.SetUp(Path.GetFileNameWithoutExtension(file), this);
        }
    }

    public void OnSelectedSceneChange(string newValue)
    {
        if (newValue == "")
            return;

        selectedScene = newValue;
        levelName = newValue;
        lobbyLevelName.text = newValue;
    }


    public void EnableEditButton(bool enable)
    {
        editButton.interactable = enable;
    }

    public void EnableRulesButton(bool enable)
    {
        rulesButton.gameObject.SetActive(enable);
    }

    public void OnReturnButtonClick()
    {
        if(lobbyPlayer.isServer)
        {
            lobbyManager.StopHost();
        }
        else
        {
            lobbyManager.StopClient();
        }
    }

    public void SetSelectedScene()
    {
        if (lobbyPlayer == null)
            Debug.Log("lobby player null");

        lobbyPlayer.UpdateSelectedScene(selectedScene);

        if (SceneUtility.GetBuildIndexByScenePath(selectedScene) != -1)
        {
            lobbyManager.playScene = selectedScene;
        }
        else
        {
            lobbyManager.playScene = "Custom";
            lobbyManager.customMapFile = selectedScene;

        }
        lobbyLevelName.text = selectedScene;
    }

    public void UpdateMapPreview()
    {
        if (lobbyLevelName.text.Contains("_"))
        {
            Regex r = new Regex(@"(\d+)_*");
            string id = r.Match(lobbyLevelName.text).Groups[1].Value;

            StartCoroutine(LoadMapPreview(id));
        }
        else
        {
            editButton.image.sprite = Resources.Load<Sprite>("Levels/Previews/" + lobbyLevelName.text);
        }
    }

    IEnumerator LoadMapPreview(string id)
    {
        WWW www = new WWW(ConfigHandler.BaseUrl + "/thumbs/" + id + ".png");
        yield return www;
        editButton.image.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
    }

    private void VersionCheck()
    {
        if (levelInfo.version != null)
        {
            int[] mapVersion = Array.ConvertAll(((string)levelInfo.version).Split('.'), s => int.Parse(s));
            int[] appVersion = Array.ConvertAll(Application.version.Split('.'), s => int.Parse(s));


            if (mapVersion[0] != appVersion[0] || mapVersion[1] != appVersion[1] || mapVersion[2] != appVersion[2])
            {
                panelVersionInfo.gameObject.SetActive(true);
            }
            else
            {
                panelListScene.SetActive(false);
                SetSelectedScene();
            }

        }
        else
        {
            panelVersionInfo.gameObject.SetActive(true);
        }
    }
}
