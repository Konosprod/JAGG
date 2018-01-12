using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

public class LobbyControls : NetworkBehaviour {

    public Transform contentPanel;
    public GameObject prefabButton;


    public Button editButton;
    public Button selectButton;
    public Text labelLevelName;
    public Text lobbyLevelName;

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
    }

    public void ListScenes()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for(int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);

            if(path.StartsWith("Assets/Scenes/Levels"))
            {
                GameObject newButton = GameObject.Instantiate(prefabButton, contentPanel);

                SceneListEntry entry = newButton.GetComponent<SceneListEntry>();
                entry.SetUp(System.IO.Path.GetFileNameWithoutExtension(path), labelLevelName, this);
            }
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
        editButton.gameObject.SetActive(enable);
    }

    public void SetSelectedScene()
    {
        lobbyPlayer.UpdateSelectedScene(selectedScene);
        lobbyManager.playScene = selectedScene;
        lobbyLevelName.text = selectedScene;
    }
}
