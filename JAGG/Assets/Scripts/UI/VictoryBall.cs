using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VictoryBall : NetworkBehaviour {

    [SyncVar]
    public string playerName;

    public Text playerNameText;

	// Use this for initialization
	void Start () {
        playerNameText.text = playerName;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
