using UnityEngine.UI;
using UnityEngine;

public class PanelJoin : MonoBehaviour {

    public Button buttonJoin;
    public IPInputField ipInput;
    public PortInputField portInput;

	// Use this for initialization
	void Start () {
        buttonJoin.interactable = false;
	}
	
	// Update is called once per frame
	void Update () {
        buttonJoin.interactable = (ipInput.isValid && portInput.isValid);
	}
}
