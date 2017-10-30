using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine;
using UnityEngine.UI;


public class PlayerStatus
{
    public int connectionId;
    public bool done;

    public PlayerStatus(int connectionId)
    {
        this.connectionId = connectionId;
        done = false;
    }
}

public class LobbyManager : NetworkLobbyManager
{
    [Header("UI Section")]
    public InputField InputIP;


    [HideInInspector]
    public List<PlayerStatus> players;

    // Use this for initialization
    void Start()
    {
        players = new List<PlayerStatus>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        players.Add(new PlayerStatus(conn.connectionId));
        return base.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if(players[i].connectionId == conn.connectionId)
            {
                players.RemoveAt(i);
            }
        }
        base.OnClientConnect(conn);
    }

    public void CreateRoom()
    {
        this.StartHost();
    }

    public void JoinRoom()
    {
        this.networkAddress = InputIP.text;
        this.StartClient();
    }
}