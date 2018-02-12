using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjImporter : MonoBehaviour {

    private static Dictionary<string, GameObject> loaded;
    private static Dictionary<string, string> paths;

	// Use this for initialization
	void Start () {
        if(loaded == null)
        {
            loaded = new Dictionary<string, GameObject>();
            paths = new Dictionary<string, string>();
        }
	}
	
    public static GameObject LoadGameObject(string path)
    {
        string key = System.IO.Path.GetFileNameWithoutExtension(path);

        if (loaded == null)
        {
            loaded = new Dictionary<string, GameObject>();
            paths = new Dictionary<string, string>();
        }

        if (!loaded.ContainsKey(key))
        {
            try
            {
                GameObject o = OBJLoader.LoadOBJFile(@path);
                o.transform.GetChild(0).gameObject.AddComponent<MeshCollider>();
                o.AddComponent<TerrainPiece>();
                o.GetComponent<TerrainPiece>().id = key;
                o.GetComponent<TerrainPiece>().prefab = false;
                o.SetActive(false);
                //Hide the "prefab" in hierarchy
                o.hideFlags = HideFlags.HideInHierarchy;

                loaded.Add(key, o);
                paths.Add(key, path);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        return loaded[key];
    }

    public static string GetObjPath(string key)
    {
        if (loaded == null)
        {
            loaded = new Dictionary<string, GameObject>();
            paths = new Dictionary<string, string>();
        }

        return paths.ContainsKey(key) ? paths[key] : "";
    }

	// Update is called once per frame
	void Update () {
		
	}
}
