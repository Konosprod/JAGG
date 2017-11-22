using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour {

    public LobbyManager lobbyManager;
    public UIManager ui;
    private Dictionary<int, GameObject> players;

    [HideInInspector]
    public bool isStarted;

    void Start()
    {
        players = new Dictionary<int, GameObject>();
    }
	
    void Update()
    {
        if (isStarted)
        {
            if (players.Count > 0)
            {
                if (AllPlayersDone())
                {
                    ResetAllPlayers();
                    foreach(GameObject o in players.Values)
                    {
                        o.GetComponent<PlayerController>().ShowScores();
                    }
                    TriggerSpawn();
                    //StartCoroutine(ui.ShowScores(5, TriggerSpawn));
                }
            }
        }
    }

    private void TriggerSpawn()
    {
        lobbyManager.SpawnNextPoint();
    }

    public bool HasPlayer()
    {
        return (players.Count > 0);
    }

    public void AddPlayer(GameObject o)
    {
        int connId = o.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
        players[connId] = o;
    }

    public void RemovePlayer(int connId)
    {
        players.Remove(connId);
    }

    public bool AllPlayersDone()
    {
        bool allDone = true;

        foreach(GameObject player in players.Values)
        {
            if (!player.GetComponent<PlayerController>().done)
                allDone = false;
        }

        return allDone;
    }

    public void ResetAllPlayers()
    {
        foreach (GameObject player in players.Values)
        {
            player.GetComponent<PlayerController>().ResetPlayer();
        }
    }

    public void ResetAllPlayersScore()
    {
        foreach(GameObject p in players.Values)
        {
            p.GetComponent<PlayerController>().score.Clear();
        }
    }

    public List<SyncListInt> GetPlayersScore()
    {
        List<SyncListInt> scores = new List<SyncListInt>();

        foreach(GameObject p in players.Values)
        {
            scores.Add(p.GetComponent<PlayerController>().score);
        }

        return scores;
    }
}
