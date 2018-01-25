using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExportLevel : MonoBehaviour {
    public Transform holes;
    public GameObject prefabEndOfGame;


    void Start()
    {
        Debug.Log(CreateCustomLevel());
    }

    public string CreateCustomLevel(string name = "", string author = "")
    {
        string json = "";
        string levelName = name;

        if (name == "")
            levelName = holes.gameObject.scene.name;


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

                h.pieces.Add(p);
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

        json = JsonUtility.ToJson(customLevel);

        return json;
    }
}
