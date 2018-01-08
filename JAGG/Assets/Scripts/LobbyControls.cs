using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class LobbyControls : MonoBehaviour {

    public Transform contentPanel;
    public GameObject prefabButton;


    public Button selectButton;
    public Text levelName;
    public Text lobbyLevelName;

    public string selectedScene;

    public LobbyManager lobbyManager;

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
                entry.SetUp(System.IO.Path.GetFileNameWithoutExtension(path), levelName, this);
            }
        }
    }

    public void SetSelectedScene()
    {
        lobbyManager.playScene = selectedScene;
        lobbyLevelName.text = selectedScene;
    }
}
