using UnityEngine.UI;
using UnityEngine;

public class PanelCreate : MonoBehaviour {

    public Button buttonCreate;
    public PortInputField portInput;


	// Use this for initialization
	void Start() {
        buttonCreate.interactable = false;
	}
	
	// Update is called once per frame
	void Update () {
        buttonCreate.interactable = portInput.isValid;
	}
}
