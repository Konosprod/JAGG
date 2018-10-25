using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelValidationStart : MonoBehaviour {

    [Header("UI")]
    public Button startValidateButton;
    public Button cancelButton;
    public GameObject panelValidationHole;

    [Header("Other")]
    public EditorManager editorManager;

    // Use this for initialization
    void Start () {
        // Buttons callbacks
        startValidateButton.onClick.AddListener(StartValidation);
        cancelButton.onClick.AddListener(Cancel);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartValidation()
    {
        this.gameObject.SetActive(false);
        panelValidationHole.SetActive(true);
    }

    public void Cancel()
    {
        this.gameObject.SetActive(false);
        editorManager.escapeMenu.gameObject.SetActive(true);
    }
}
