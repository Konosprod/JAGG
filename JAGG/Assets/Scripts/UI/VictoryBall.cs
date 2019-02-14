using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


#pragma warning disable CS0618 // Le type ou le membre est obsolète

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
