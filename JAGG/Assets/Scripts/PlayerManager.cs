using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour {

    public LobbyManager lobbyManager;
    public UIManager ui;
    private Dictionary<int, GameObject> players;

    private SyncListInt scoreP1 = new SyncListInt();
    private SyncListInt scoreP2 = new SyncListInt();
    private SyncListInt scoreP3 = new SyncListInt();
    private SyncListInt scoreP4 = new SyncListInt();

    [SyncVar]
    private int nbPlayers = 0;


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

                    // Update scores
                    int i = 0;
                    foreach (GameObject p in players.Values)
                    {
                        SyncListInt scp = p.GetComponent<PlayerController>().score;
                        if (i == 0)
                        {
                            if (scoreP1.Count < scp.Count)
                            {
                                for (int k = scoreP1.Count; k < scp.Count; k++)
                                {
                                    scoreP1.Add(scp[k]);
                                }
                            }
                        }
                        else if (i == 1)
                        {
                            if (scoreP2.Count < scp.Count)
                            {
                                for (int k = scoreP2.Count; k < scp.Count; k++)
                                {
                                    scoreP2.Add(scp[k]);
                                }
                            }
                        }
                        else if(i == 2)
                        {
                            if (scoreP3.Count < scp.Count)
                            {
                                for (int k = scoreP3.Count; k < scp.Count; k++)
                                {
                                    scoreP3.Add(scp[k]);
                                }
                            }
                        }
                        else if(i == 3)
                        {
                            if (scoreP4.Count < scp.Count)
                            {
                                for (int k = scoreP4.Count; k < scp.Count; k++)
                                {
                                    scoreP4.Add(scp[k]);
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError("Plus de 4 joueurs ? NANI ?!");
                        }

                        i++;
                    }


                    foreach (GameObject o in players.Values)
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

    public void MovePlayersTo(Transform nextPosition)
    {

        foreach(GameObject o in players.Values)
        {
            o.transform.position = nextPosition.position;
            o.GetComponent<PlayerController>().EnablePlayer();
        }
    }

    public bool HasPlayer()
    {
        return (players.Count > 0);
    }

    public void ClearPlayers()
    {
        players.Clear();
        scoreP1.Clear();
        scoreP2.Clear();
        scoreP3.Clear();
        scoreP4.Clear();
        nbPlayers = 0;
    }

    public void AddPlayer(GameObject o, int connId = -1)
    {
        if(connId == -1)
            connId = o.GetComponent<NetworkIdentity>().connectionToClient.connectionId;

        players[connId] = o;

        nbPlayers++;
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

        List<SyncListInt> sc = new List<SyncListInt>();

        if (nbPlayers >= 1)
            sc.Add(scoreP1);
        if (nbPlayers >= 2)
            sc.Add(scoreP2);
        if (nbPlayers >= 3)
            sc.Add(scoreP3);
        if (nbPlayers >= 4)
            sc.Add(scoreP4);

        return sc;
    }
}
