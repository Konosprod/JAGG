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

        for (int i = 0; i < level.holes.Count; i++)
        {
            Hole h = level.holes[i];
            GameObject hole = new GameObject("Hole " + (i + 1).ToString());

            hole.transform.SetParent(holes.transform);

            GameObject startPoint = new GameObject("Spawn Point");
            startPoint.transform.SetParent(hole.transform);

            startPoint.transform.position = h.properties.spawnPoint;

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

            //Last holes needs end of game flag
            if(i == level.holes.Count - 1)
                levelProperties.nextSpawnPoint = endOfGame.transform;
        }

        /*
        CustomLevel level = new CustomLevel
        {
            holes = new System.Collections.Generic.List<Hole>(),
            author = "test",
            name = "test"
        };

        Hole h = new Hole
        {
            pieces = new System.Collections.Generic.List<Piece>(),
            properties = new HoleInfo()
        };

        h.properties.maxShot = 6;
        h.properties.maxTime = 60;
        h.properties.par = 2;
        h.properties.spawnPoint = new Vector3(0, 0.2f, 0);

        h.pieces.Add(new Piece {
            id = "Start",
            rotation = new Vector3(0, 0, 0),
            position = new Vector3(0, 0, 0),
            scale = new Vector3(1, 1, 1)
        });

        h.pieces.Add(new Piece {
            id = "HoleEnd",
            rotation = new Vector3(0, 270, 0),
            position = new Vector3(0, 0, 2),
            scale = new Vector3(1, 1, 1)
        });

        level.holes.Add(h);

        string json = JsonUtility.ToJson(level, true);

        File.WriteAllText(Path.Combine(Path.Combine(Application.persistentDataPath, "levels"), "test.json"), json);
        */
    }

    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
