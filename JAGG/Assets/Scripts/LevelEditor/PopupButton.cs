using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupButton : MonoBehaviour {

    public GameObject go;
    public Button button;

	// Use this for initialization
	void Start () {
        button.onClick.AddListener(Popup);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void Popup()
    {
        bool state = go.activeSelf;

        go.SetActive(!state);
    }
}
