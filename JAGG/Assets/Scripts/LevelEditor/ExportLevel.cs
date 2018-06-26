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
    private EditorManager editorManager;

    void Start()
    {
        editorManager = gameObject.GetComponent<EditorManager>();
        //Debug.Log(CreateCustomLevel());
        //SaveLevel();
    }

    //Test function
    public void SaveLevel()
    {
        //CreateCustomLevel("TestLevel", "TestAuthor", Path.Combine(Application.persistentDataPath, "Levels"));
    }

    public byte[] CreateCustomLevel(string name = "", string author = "", bool checkHoleValidity = false)
    {
        string levelName = name;

        if (name == "")
            levelName = holes.gameObject.scene.name;

        ZipFile mapFile = new ZipFile(levelName + ".map");

        mapFile.AddDirectoryByName("obj");

        JObject customLevel = new JObject();

        customLevel.Add("author", author);
        customLevel.Add("name", levelName);
        customLevel.Add("version", Application.version);

        JArray jholes = new JArray();

        if (checkHoleValidity)
        {
            int i = 0;
            while (i < 18)
            {
                bool isvalid = editorManager.isHoleValid(i);

                if (isvalid)
                {
                    Transform holeTransform = holes.GetChild(i);
                    GameObject hole = holeTransform.gameObject;

                    Transform spawnPoint = holeTransform.Find("Spawn Point").transform;

                    JObject h = new JObject();
                    JArray jPieces = new JArray();

                    TerrainPiece[] pieces = hole.GetComponentsInChildren<TerrainPiece>();

                    for(int j = 0; j < pieces.Length; j++)
                    {
                        TerrainPiece piece = pieces[j];

                        piece.number = j;

                        if(piece.transform.parent != holeTransform)
                        {
                            piece.parentNumber = j - 1;
                        }

                        piece.ToJson(jPieces);

                        if (!piece.prefab)
                        {
                            //Copy .obj, .mtl, .png to obj/
                            string path = Path.GetDirectoryName(ObjImporter.GetObjPath(piece.id)) + Path.DirectorySeparatorChar;

                            mapFile.AddFile(path + piece.id + ".obj", "obj");
                            mapFile.AddFile(path + piece.id + ".mtl", "obj");
                            mapFile.AddFile(path + piece.id + ".png", "obj");
                        }
                    }
                    /*
                    foreach (TerrainPiece piece in pieces)
                    {
                        piece.ToJson(jPieces);

                        if (!piece.prefab)
                        {
                            //Copy .obj, .mtl, .png to obj/
                            string path = Path.GetDirectoryName(ObjImporter.GetObjPath(piece.id)) + Path.DirectorySeparatorChar;

                            mapFile.AddFile(path + piece.id + ".obj", "obj");
                            mapFile.AddFile(path + piece.id + ".mtl", "obj");
                            mapFile.AddFile(path + piece.id + ".png", "obj");
                        }
                    }*/

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
                i++;
            }
        }
        else
        {
            foreach (Transform holeTransform in holes)
            {
                GameObject hole = holeTransform.gameObject;

                Transform spawnPoint = holeTransform.Find("Spawn Point").transform;

                JObject h = new JObject();
                JArray jPieces = new JArray();

                TerrainPiece[] pieces = hole.GetComponentsInChildren<TerrainPiece>();

                for (int j = 0; j < pieces.Length; j++)
                {
                    TerrainPiece piece = pieces[j];

                    piece.number = j;

                    if (piece.transform.parent != holeTransform)
                    {
                        piece.parentNumber = j - 1;
                    }

                    piece.ToJson(jPieces);

                    if (!piece.prefab)
                    {
                        //Copy .obj, .mtl, .png to obj/
                        string path = Path.GetDirectoryName(ObjImporter.GetObjPath(piece.id)) + Path.DirectorySeparatorChar;

                        mapFile.AddFile(path + piece.id + ".obj", "obj");
                        mapFile.AddFile(path + piece.id + ".mtl", "obj");
                        mapFile.AddFile(path + piece.id + ".png", "obj");
                    }
                }

                /*
                foreach (TerrainPiece piece in pieces)
                {
                    Debug.Log(piece.transform.parent.transform == holeTransform);

                    piece.ToJson(jPieces);

                    if (!piece.prefab)
                    {
                        //Copy .obj, .mtl, .png to obj/
                        string path = Path.GetDirectoryName(ObjImporter.GetObjPath(piece.id)) + Path.DirectorySeparatorChar;

                        mapFile.AddFile(path + piece.id + ".obj", "obj");
                        mapFile.AddFile(path + piece.id + ".mtl", "obj");
                        mapFile.AddFile(path + piece.id + ".png", "obj");
                    }
                }
                */

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
        }

        customLevel.Add("holes", jholes);

        Debug.Log(customLevel);

        MemoryStream ms = new MemoryStream();
        MemoryStream outZip = new MemoryStream();

        using (BsonWriter writer = new BsonWriter(ms))
        {
            customLevel.WriteTo(writer);
        }

        mapFile.AddEntry("level.json", ms.ToArray());

        mapFile.Save(outZip);
        outZip.Seek(0, SeekOrigin.Begin);
        //mapFile.Save(location + Path.DirectorySeparatorChar + levelName + ".map");

        return outZip.ToArray();
    }
}
