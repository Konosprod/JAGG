using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjImporter : MonoBehaviour {

    private static Dictionary<string, GameObject> loaded;

	// Use this for initialization
	void Start () {
        if(loaded == null)
        {
            loaded = new Dictionary<string, GameObject>();
        }
	}
	
    public void LoadGameObject(string path)
    {
        string key = System.IO.Path.GetFileNameWithoutExtension(path);

        Debug.Log(@path);

        if (!loaded.ContainsKey(key))
        {

            try
            {
                GameObject o = OBJLoader.LoadOBJFile(@path);
                o.SetActive(false);
                loaded.Add(key, o);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        Instantiate(loaded[key]).SetActive(true);
    }

	// Update is called once per frame
	void Update () {
		
	}
}
