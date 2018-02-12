using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ionic.Zip;
using System.IO;

public class ExportLevel : MonoBehaviour {
    public Transform holes;
    public GameObject prefabEndOfGame;


    void Start()
    {
        //Debug.Log(CreateCustomLevel());
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

        CustomLevel customLevel = new CustomLevel
        {
            author = author,
            name = levelName,
            holes = new List<Hole>()
        };

        foreach(Transform holeTransform in holes)
        {
            GameObject hole = holeTransform.gameObject;

            Transform spawnPoint = holeTransform.Find("Spawn Point").transform;

            Hole h = new Hole
            {
                pieces = new List<Piece>()
            };

            TerrainPiece[] pieces = hole.GetComponentsInChildren<TerrainPiece>();

            foreach (TerrainPiece piece in pieces)
            {

                Piece p = new Piece
                {
                    id = piece.id,
                    position = piece.gameObject.transform.position,
                    rotation = piece.gameObject.transform.eulerAngles,
                    scale = piece.gameObject.transform.localScale
                };

                if(piece.id == "BoosterPad")
                {
                    p.addFactor = piece.gameObject.GetComponent<BoosterPad>().addFactor;
                    p.multFactor = piece.gameObject.GetComponent<BoosterPad>().multFactor;
                }

                h.pieces.Add(p);

                if(!piece.prefab)
                {
                    //Copy .obj, .mtl, .png to obj/
                    string path = Path.GetDirectoryName(ObjImporter.GetObjPath(piece.id)) + Path.DirectorySeparatorChar;
                    mapFile.AddFile(path + p.id + ".obj", "obj");
                    mapFile.AddFile(path + p.id + ".mtl", "obj");
                    mapFile.AddFile(path + p.id + ".png", "obj");
                }
            }


            LevelProperties properties = hole.GetComponentInChildren<LevelProperties>();

            h.properties = new HoleInfo
            {
                maxShot = properties.maxShot,
                maxTime = properties.maxTime,
                par = properties.par,
                spawnPoint = spawnPoint.position
            };

            customLevel.holes.Add(h);
        }

        //level.json
        json = JsonUtility.ToJson(customLevel);

        mapFile.AddEntry("level.json", json);
        mapFile.Save(location + Path.DirectorySeparatorChar + levelName + ".map");

        //Create file .map

        return json;
    }
}
