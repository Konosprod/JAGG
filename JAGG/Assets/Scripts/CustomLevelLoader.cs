using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;
using System;


#pragma warning disable CS0618 // Le type ou le membre est obsolète

public class CustomLevelLoader : MonoBehaviour {

    public GameObject holes;

    [HideInInspector]
    public string steamid = "";
    [HideInInspector]
    public int mapid = 0;

    private List<GameObject> loadedPieces;
    private EditorManager editorManager;

    private Dictionary<string, GameObject> cachePiece = new Dictionary<string, GameObject>();

    // Use this for initialization
    void Awake()
    {
 
    }

    public void LoadLevel(string path, EditorManager manager)
    {
        if (loadedPieces == null)
            loadedPieces = new List<GameObject>();

        editorManager = manager;

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

        if(level["steamid"] != null)
            steamid = level["steamid"].ToString();

        if(level["mapid"] != null)
            mapid = (int)level["mapid"];

        if(level["version"] != null)
        {
            int[] mapVersion = Array.ConvertAll(((string)level["version"]).Split('.'), s => int.Parse(s));
            int[] appVersion = Array.ConvertAll(Application.version.Split('.'), s => int.Parse(s));


            if(mapVersion[0] < appVersion[0] || mapVersion[1] < appVersion[1] || mapVersion[2] < appVersion[2])
            {
                Debug.Log("It's an old map, might have bugs");
            }
            else if(mapVersion[0] > appVersion[0] || mapVersion[1] > appVersion[1] || mapVersion[2] > appVersion[2])
            {
                Debug.Log("It's a map from the future");
            }
            else
            {

            }

            Debug.Log("Map version : " + level["version"] + " Application version : " + Application.version);

        }
        else
        {
            Debug.Log("Map without version number, might have bugs");
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

            GameObject startPoint = null;

            Transform hasStartPoint = hole.transform.Find("Spawn Point");

            if (!hasStartPoint)
                startPoint = new GameObject("Spawn Point");
            else
                startPoint = hasStartPoint.gameObject;

            startPoint.transform.SetParent(hole.transform);

            startPoint.transform.position = new Vector3((float)jHole["properties"]["spawnPoint"]["x"], (float)jHole["properties"]["spawnPoint"]["y"], (float)jHole["properties"]["spawnPoint"]["z"]);

            spawnPositions.Add(startPoint);

            if (i == 0)
            {
                if(startPoint.GetComponent<NetworkStartPosition>() == null)
                    startPoint.AddComponent<NetworkStartPosition>();
            }

            JArray pieces = jHole["pieces"] as JArray;

            for(int j = 0; j < pieces.Count; j++)
            {
                JObject jPiece = pieces[j] as JObject;
                GameObject objectToLoad = null;

                objectToLoad = LoadPiece(jPiece["id"].ToString());


                if (objectToLoad == null)
                {
                    //Debug.Log("Unable to load : " + jPiece["id"]);
                    objectToLoad = ObjImporter.LoadGameObject(Path.Combine(tmpPath, "obj" + Path.DirectorySeparatorChar + jPiece["id"] + ".obj"));

                    if(manager != null)
                    {
                        editorManager.LoadCustomObject(Path.Combine(tmpPath, "obj" + Path.DirectorySeparatorChar + jPiece["id"] + ".obj"));
                    }
                }

                GameObject o = Instantiate(objectToLoad, hole.transform);

                if((int)jPiece["parentNumber"] != -1)
                    o.transform.parent = loadedPieces[j - 1].transform;

                o.GetComponent<TerrainPiece>().FromJson(jPiece.ToString());

                if (o == null)
                    Debug.Log("merde");

                o.SetActive(true);
                loadedPieces.Add(o);
            }
            /*
            foreach (JObject jPiece in jHole["pieces"])
            {
                GameObject objectToLoad = null;

                objectToLoad = LoadPiece(jPiece["id"].ToString());


                if (objectToLoad == null)
                {
                    objectToLoad = ObjImporter.LoadGameObject(Path.Combine(tmpPath, "obj" + Path.DirectorySeparatorChar + jPiece["id"] + ".obj"));
                }

                GameObject o = Instantiate(objectToLoad, hole.transform);

                o.GetComponent<TerrainPiece>().FromJson(jPiece.ToString());

                o.SetActive(true);
            }
            */

            GameObject goLevelProp = null;
            Transform hasLevelProperties = hole.transform.Find("Level Properties");

            if (!hasLevelProperties)
            {
                goLevelProp = Instantiate(Resources.Load("Prefabs/Level Properties") as GameObject, hole.transform);
                goLevelProp.name = "Level Properties"; //Remove the (Clone) shit
                goLevelProp.transform.SetParent(hole.transform);
            }
            else
            {
                goLevelProp = hasLevelProperties.gameObject;
            }

            LevelProperties levelProperties = goLevelProp.GetComponent<LevelProperties>();

            levelProperties.maxShot = (int)jHole["properties"]["maxShot"];
            levelProperties.maxTime = (int)jHole["properties"]["maxTime"];
            levelProperties.par = (int)jHole["properties"]["par"];

            i++;
        }

        for (int j = 1; j < ((JArray)level["holes"]).Count; j++)
        {
            holes.transform.GetChild(j - 1).GetComponentInChildren<LevelProperties>().nextSpawnPoint = spawnPositions[j].transform;
        }
        holes.transform.GetChild(((JArray)level["holes"]).Count - 1).GetComponentInChildren<LevelProperties>().nextSpawnPoint = endOfGame.transform;

        loadedPieces.Clear();
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
