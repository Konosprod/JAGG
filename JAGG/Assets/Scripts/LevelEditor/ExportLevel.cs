using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ionic.Zip;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Bson;

public class ExportLevel : MonoBehaviour {
    public Transform holes;
    public GameObject prefabEndOfGame;


    void Start()
    {
        //Debug.Log(CreateCustomLevel());
        //SaveLevel();
    }

    //Test function
    public void SaveLevel()
    {
        CreateCustomLevel("TestLevel", "TestAuthor", Path.Combine(Application.persistentDataPath, "Levels"));
    }

    public string CreateCustomLevel(string name = "", string author = "", string location = "")
    {
        string json = "";
        string levelName = name;

        if (name == "")
            levelName = holes.gameObject.scene.name;

        ZipFile mapFile = new ZipFile(levelName + ".map");

        mapFile.AddDirectoryByName("obj");

        JObject customLevel = new JObject();

        customLevel.Add("author", author);
        customLevel.Add("name", levelName);

        JArray jholes = new JArray();

        foreach(Transform holeTransform in holes)
        {
            GameObject hole = holeTransform.gameObject;

            Transform spawnPoint = holeTransform.Find("Spawn Point").transform;

            JObject h = new JObject();
            JArray jPieces = new JArray();

            TerrainPiece[] pieces = hole.GetComponentsInChildren<TerrainPiece>();

            foreach (TerrainPiece piece in pieces)
            {
                piece.ToJson(jPieces);

                if(!piece.prefab)
                {
                    //Copy .obj, .mtl, .png to obj/
                    string path = Path.GetDirectoryName(ObjImporter.GetObjPath(piece.id)) + Path.DirectorySeparatorChar;
                    
                    mapFile.AddFile(path + piece.id + ".obj", "obj");
                    mapFile.AddFile(path + piece.id + ".mtl", "obj");
                    mapFile.AddFile(path + piece.id + ".png", "obj");
                }
            }

            h.Add("pieces", jPieces);

            LevelProperties properties = hole.GetComponentInChildren<LevelProperties>();

            JObject jProperties = new JObject
            {
                { "maxShot", properties.maxShot },
                { "maxTime", properties.maxTime },
                { "par", properties.par }
            };

            JObject jSpawnPoint = new JObject();
            jSpawnPoint.Add("x", spawnPoint.position.x);
            jSpawnPoint.Add("y", spawnPoint.position.y);
            jSpawnPoint.Add("z", spawnPoint.position.z);

            jProperties.Add("spawnPoint", jSpawnPoint);
            h.Add("properties", jProperties);
            jholes.Add(h);
        }

        customLevel.Add("holes", jholes);

        MemoryStream ms = new MemoryStream();

        using (BsonWriter writer = new BsonWriter(ms))
        {
            customLevel.WriteTo(writer);
        }

        mapFile.AddEntry("level.json", ms.ToArray());
        mapFile.Save(location + Path.DirectorySeparatorChar + levelName + ".map");

        return json;
    }
}
