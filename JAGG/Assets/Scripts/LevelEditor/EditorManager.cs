using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour {


    public GameObject scrollviewContent;

    private GameObject[] prefabs;

	// Use this for initialization
	void Start () {
        prefabs = Resources.LoadAll<GameObject>("Prefabs/Terrain");
        foreach (GameObject pref in prefabs)
        {
            //Debug.Log(pref.name);
            string preview = Application.dataPath + "/Resources/Previews/" + pref.name + "Preview.png";
            GameObject previewImage = new GameObject(pref.name+"Preview");
            previewImage.AddComponent<RectTransform>();
            previewImage.AddComponent<LayoutElement>();
            Image pi_im = previewImage.AddComponent<Image>();
            pi_im.sprite = Resources.Load<Sprite>("Previews/" + pref.name + "Preview");


            previewImage.transform.SetParent(scrollviewContent.transform);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
