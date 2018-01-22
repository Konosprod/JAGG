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

    public LobbyControls lobbyControls;

	// Use this for initialization
	void Start () {
        LobbyPlayerList._instance.AddPlayer(this);

        lobbyControls = GameObject.FindObjectOfType<LobbyControls>();

        if (isLocalPlayer)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupOtherPlayer();
        }
    }

    public void ResetStatus()
    {
        if (isLocalPlayer)
        {
            SendNotReadyToBeginMessage();
            CmdResetStatus();
        }
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

        if (isServer)
        {
            lobbyControls.EnableEditButton(isServer);
        }

        lobbyControls.lobbyPlayer = this;
    }

    public void UpdateSelectedScene(string value)
    {
        CmdUpdateSelectedScene(value);
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

    [Command]
    public void CmdResetStatus()
    {
        toggleReady.isOn = false;
        SendNotReadyToBeginMessage();
    }

    [Command]
    public void CmdUpdateSelectedScene(string value)
    {
        lobbyControls.labelLevelName.text = value;
        lobbyControls.lobbyLevelName.text = value;
        RpcUpdateSelectedScene(value);
    }

#endregion

    [ClientRpc]
    public void RpcUpdateSelectedScene(string value)
    {
        lobbyControls.labelLevelName.text = value;
        lobbyControls.lobbyLevelName.text = value;
        LobbyManager._instance.customMapFile = value;
    }
}
