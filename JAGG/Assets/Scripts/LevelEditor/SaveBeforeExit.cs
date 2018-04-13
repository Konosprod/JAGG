using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveBeforeExit : MonoBehaviour {

    public EditorManager editorManager;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SaveAndQuit()
    {
        editorManager.panelExport.saveCallback = delegate ()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        };
        editorManager.panelExport.gameObject.SetActive(true);
    }

    public void DiscardAndQuit()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

}
