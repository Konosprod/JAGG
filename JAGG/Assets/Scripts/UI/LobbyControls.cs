﻿using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

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
                GameObject newButton = Instantiate(prefabButton, contentPanel);

                SceneListEntry entry = newButton.GetComponent<SceneListEntry>();
                entry.SetUp(Path.GetFileNameWithoutExtension(path), labelLevelName, this);
            }
        }

        if(!Directory.Exists(Path.Combine(Application.persistentDataPath, "levels")))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "levels"));
        }

        foreach(string file in Directory.GetFiles(Path.Combine(Application.persistentDataPath, "levels"), "*.json"))
        {
            GameObject newButton = Instantiate(prefabButton, contentPanel);

            SceneListEntry entry = newButton.GetComponent<SceneListEntry>();
            entry.SetUp(Path.GetFileNameWithoutExtension(file), labelLevelName, this);
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
}