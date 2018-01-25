using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class CustomLevelLoader : MonoBehaviour {

    public GameObject holes;

    // Use this for initialization
    void Awake()
    {
        string levelDirectory = Path.Combine(Application.persistentDataPath, "levels");

        if(!Directory.Exists(levelDirectory))
        {
            Directory.CreateDirectory(levelDirectory);
        }

        string json = File.ReadAllText(Path.Combine(levelDirectory, LobbyManager._instance.customMapFile +".json"));

        GameObject endOfGame = Resources.Load("Prefabs/EndOfGamePosition") as GameObject;

        CustomLevel level = JsonUtility.FromJson<CustomLevel>(json);

        List<GameObject> spawnPositions = new List<GameObject>();

        //
        for (int i = 0; i < level.holes.Count; i++)
        {
            Hole h = level.holes[i];
            GameObject hole = new GameObject("Hole " + (i + 1).ToString());

            hole.transform.SetParent(holes.transform);

            GameObject startPoint = new GameObject("Spawn Point");
            startPoint.transform.SetParent(hole.transform);

            startPoint.transform.position = h.properties.spawnPoint;
            spawnPositions.Add(startPoint);

            //First hole needs a network start position
            if (i == 0)
            {
                startPoint.AddComponent<NetworkStartPosition>();
            }

            foreach (Piece p in h.pieces)
            {
                GameObject o = GameObject.Instantiate<GameObject>(Resources.Load("Prefabs/Terrain/" + p.id) as GameObject, hole.transform);
                o.transform.position = p.position;
                o.transform.localEulerAngles = p.rotation;
                o.transform.localScale = p.scale;
            }

            GameObject goLevelProp = GameObject.Instantiate(Resources.Load("Prefabs/Level Properties") as GameObject, hole.transform);

            LevelProperties levelProperties = goLevelProp.GetComponent<LevelProperties>();

            levelProperties.maxShot = h.properties.maxShot;
            levelProperties.maxTime = h.properties.maxTime;
            levelProperties.par = h.properties.par;
        }

        for (int i = 1; i < holes.transform.childCount; i++)
        {
            holes.transform.GetChild(i-1).GetComponentInChildren<LevelProperties>().nextSpawnPoint = spawnPositions[i].transform;

            if (i == level.holes.Count - 1)
            {
                holes.transform.GetChild(i).GetComponentInChildren<LevelProperties>().nextSpawnPoint = endOfGame.transform;
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
