using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CustomScene : MonoBehaviour {

    public CustomLevelLoader levelLoader;

    void Awake()
    {
        string levelDirectory = Path.Combine(Application.persistentDataPath, "Levels");

        if (!Directory.Exists(levelDirectory))
        {
            Directory.CreateDirectory(levelDirectory);
        }

        levelLoader.LoadLevel(Path.Combine(levelDirectory, LobbyManager._instance.customMapFile + ".map"), null);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
