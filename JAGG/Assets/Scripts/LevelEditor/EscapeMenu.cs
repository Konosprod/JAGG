using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour {

    public EditorManager editorManager;

    public bool isDone = true;

    [Header("Buttons")]
    public Button loadButton;
    public Button saveButton;
    public Button returnButton;
    public Button optionsButton;
    public Button quitButton;

    [Header("UI Logic")]
    public PanelExport panelExport;
    public FileBrowser fileBrowser;

	// Use this for initialization
	void Start () {
        loadButton.onClick.AddListener(Popup);
        saveButton.onClick.AddListener(Popup);
        optionsButton.onClick.AddListener(Popup);
	}
	
	// Update is called once per frame
	void Update () {
		if(!panelExport.gameObject.activeSelf && !fileBrowser.gameObject.activeSelf && !SettingsManager._instance.optionsPanel.activeSelf  && !isDone)
        {
            isDone = true;
        }
	}

    public void Popup()
    {
        isDone = false;
    }
}
