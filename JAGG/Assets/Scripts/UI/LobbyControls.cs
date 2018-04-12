using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

public class LobbyControls : NetworkBehaviour {

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

    [Header("Game Logic")]
    public string selectedScene;

    [SyncVar(hook ="OnSelectedSceneChange")]
    public string levelName;

    public LobbyManager lobbyManager;

    public LobbyPlayer lobbyPlayer;

    void Start()
    {
        ListScenes();
    }

    void OnEnable()
    {
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(SetSelectedScene);

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
                entry.SetUp(Path.GetFileNameWithoutExtension(path), labelLevelName, imageScenePreview, this);
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
            entry.SetUp(Path.GetFileNameWithoutExtension(file), labelLevelName, imageScenePreview, this);
        }
    }

    public void OnSelectedSceneChange(string newValue)
    {
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
        Regex r = new Regex(@"(\d+)_*");
        string id = r.Match(lobbyLevelName.text).Groups[1].Value;

        StartCoroutine(LoadMapPreview(id));
    }

    IEnumerator LoadMapPreview(string id)
    {
        WWW www = new WWW("https://jagg.konosprod.fr/thumbs/" + id + ".png");
        yield return www;
        editButton.image.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
    }
}
