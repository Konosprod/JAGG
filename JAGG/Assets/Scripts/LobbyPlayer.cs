using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;

public class LobbyPlayer : NetworkLobbyPlayer {

    public Text playerNameLabel;
    public Toggle toggleReady;
    public Text readyText;

    public Color localPlayerColor = new Color(1, 1, 1);

    [SyncVar(hook = "OnMyName")]
    public string playerName = "";

	// Use this for initialization
	void Start () {
        LobbyPlayerList._instance.AddPlayer(this);

        if (isLocalPlayer)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupOtherPlayer();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void OnStartLocalPlayer()
    {
        //Set buttons interactable
    }

    public override void OnClientEnterLobby()
    {
        OnMyName(playerName);
    }

    private void SetupLocalPlayer()
    {
        toggleReady.onValueChanged.RemoveAllListeners();
        toggleReady.onValueChanged.AddListener(OnReadyClicked);

        if (playerName == "")
            CmdNameChanged("Player " + (LobbyPlayerList._instance.playerListContentTransform.childCount).ToString());
    }

    private void SetupOtherPlayer()
    {
        toggleReady.interactable = false;

        //playerName.text = "Player " + LobbyManager.instance.numPlayers.ToString();
    }

    private void OnMyName(string newName)
    {
        playerName = newName;
        playerNameLabel.text = playerName;

        if(isLocalPlayer)
        {
            playerNameLabel.color = localPlayerColor;
        }
    }

    private void OnReadyClicked(bool newValue)
    {
        if (newValue == true)
        {
            SendReadyToBeginMessage();
        }
        else
        {
            SendNotReadyToBeginMessage();
        }
    }

    public override void OnClientReady(bool readyState)
    {
        if(readyState)
        {
            toggleReady.isOn = true;
            readyText.text = "Ready";
        }
        else
        {
            toggleReady.isOn = false;
            readyText.text = "Not Ready";
        }
    }

    #region Command

    [Command]
    public void CmdNameChanged(string name)
    {
        playerName = name;
    }

    public void CmdResetStatus()
    {
        toggleReady.isOn = false;
    }

#endregion
}
