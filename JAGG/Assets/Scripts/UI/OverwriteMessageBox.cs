using UnityEngine;
using UnityEngine.UI;

public class OverwriteMessageBox : MonoBehaviour {

    public FileSaver fileSaver;
    public Text filenameText;
    public Button yesButton;
    public Button noButton;

    [HideInInspector]
    public string path;

	// Use this for initialization
	void Start () {

        yesButton.onClick.AddListener(delegate ()
        {
            gameObject.SetActive(false);
            fileSaver.SaveObject(path, true);
        });

        noButton.onClick.AddListener(delegate ()
        {
            gameObject.SetActive(false);
            fileSaver.EnableButtons();
        });


        filenameText.text = path;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
