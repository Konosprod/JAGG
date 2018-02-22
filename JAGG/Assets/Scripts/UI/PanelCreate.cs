using UnityEngine.UI;
using UnityEngine;

public class PanelCreate : MonoBehaviour {

    public Button buttonCreate;
    public PortInputField portInput;

    public bool shouldCheckInputs = true;


	// Use this for initialization
	void Start() {
        buttonCreate.interactable = false;
	}
	
	// Update is called once per frame
	void Update () {
        if(shouldCheckInputs)
            buttonCreate.interactable = portInput.isValid;
	}
}
