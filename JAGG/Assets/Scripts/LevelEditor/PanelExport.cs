using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelExport : MonoBehaviour {

    public Image imagePreview;
    public InputField tagsInput;
    public InputField nameInput;
    public Button saveLocalButton;
    public Button uploadButton;

	// Use this for initialization
	void Start () {
        uploadButton.onClick.AddListener(UploadMap);
        saveLocalButton.onClick.AddListener(SaveLocal);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UploadMap()
    {
        this.gameObject.SetActive(false);
    }

    public void SaveLocal()
    {
        this.gameObject.SetActive(false);
    }
}
