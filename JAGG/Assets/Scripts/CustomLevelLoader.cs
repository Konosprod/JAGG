using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;

public class CustomLevelLoader : MonoBehaviour {

    public GameObject holes;

    private Dictionary<string, GameObject> cachePiece = new Dictionary<string, GameObject>();

    // Use this for initialization
    void Awake()
    {
 
    }
    

    public void LoadLevel(string path)
    {
        //ZipFile mapFile = new ZipFile(Path.Combine(levelDirectory, LobbyManager._instance.customMapFile + ".map"));
        ZipFile mapFile = new ZipFile(path);

        //Don't forget to create the directory maybe
        string tmpPath = Path.Combine(Application.temporaryCachePath, Path.GetFileName(path));
        //string tmpPath = Path.Combine(Application.temporaryCachePath, "test");

        mapFile.ExtractAll(tmpPath, ExtractExistingFileAction.OverwriteSilently);

        byte[] data = File.ReadAllBytes(Path.Combine(tmpPath, "level.json"));

        MemoryStream ms = new MemoryStream(data);

        GameObject endOfGame = Resources.Load("Prefabs/EndOfGamePosition") as GameObject;

        JObject level = null;

        using (BsonReader reader = new BsonReader(ms))
        {
            level = (JObject)JToken.ReadFrom(reader);
        }

        List<GameObject> spawnPositions = new List<GameObject>();

        int i = 0;
        foreach (JObject jHole in level["holes"])
        {
            GameObject hole = null;

            Transform transform = holes.transform.Find("Hole " + (i + 1).ToString());
            if (transform != null)
                hole = transform.gameObject;
            else
                hole = new GameObject("Hole " + (i + 1).ToString());

            hole.transform.SetParent(holes.transform);

            GameObject startPoint = new GameObject("Spawn Point");
            startPoint.transform.SetParent(hole.transform);

            startPoint.transform.position = new Vector3((float)jHole["properties"]["spawnPoint"]["x"], (float)jHole["properties"]["spawnPoint"]["y"], (float)jHole["properties"]["spawnPoint"]["z"]);

            spawnPositions.Add(startPoint);

            if (i == 0)
            {
                startPoint.AddComponent<NetworkStartPosition>();
            }

            foreach (JObject jPiece in jHole["pieces"])
            {
                GameObject objectToLoad = null;

                objectToLoad = LoadPiece(jPiece["id"].ToString());


                if (objectToLoad == null)
                {
                    objectToLoad = ObjImporter.LoadGameObject(Path.Combine(tmpPath, "obj" + Path.DirectorySeparatorChar + jPiece["id"] + ".obj"));
                }

                GameObject o = GameObject.Instantiate<GameObject>(objectToLoad, hole.transform);

                o.GetComponent<TerrainPiece>().FromJson(jPiece.ToString());

                o.SetActive(true);
            }

            GameObject goLevelProp = GameObject.Instantiate(Resources.Load("Prefabs/Level Properties") as GameObject, hole.transform);

            LevelProperties levelProperties = goLevelProp.GetComponent<LevelProperties>();

            levelProperties.maxShot = (int)jHole["properties"]["maxShot"];
            levelProperties.maxTime = (int)jHole["properties"]["maxTime"];
            levelProperties.par = (int)jHole["properties"]["par"];

            i++;
        }

        for (int j = 1; j < holes.transform.childCount; j++)
        {
            holes.transform.GetChild(j - 1).GetComponentInChildren<LevelProperties>().nextSpawnPoint = spawnPositions[j].transform;
        }
        holes.transform.GetChild(((JArray)level["holes"]).Count - 1).GetComponentInChildren<LevelProperties>().nextSpawnPoint = endOfGame.transform;
    }

    // Update is called once per frame
    void Update () {
		
	}

    private GameObject LoadPiece(string id)
    {
        GameObject ret = null;

        if (cachePiece.ContainsKey(id))
        {
            return cachePiece[id];
        }


        ret = Resources.Load("Prefabs/Terrain/" + id) as GameObject;

        cachePiece.Add(id, ret);

        return ret;
    }
}
