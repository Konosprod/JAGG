﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : NetworkBehaviour {

    public UIManager ui;
    private Dictionary<int, GameObject> players;

    private SyncListInt scoreP1 = new SyncListInt();
    private SyncListInt scoreP2 = new SyncListInt();
    private SyncListInt scoreP3 = new SyncListInt();
    private SyncListInt scoreP4 = new SyncListInt();

    private static PlayerManager _instance;

    [SyncVar]
    private int nbPlayers = 0;


    [HideInInspector]
    public bool isStarted;

    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

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

                    LobbyManager._instance.StopTimer();

                    ShowPlayersScores();

                    Invoke("TriggerSpawn", 5);
                }
            }
        }
    }

    public List<GameObject> GetPodium()
    {
        List<GameObject> podium = new List<GameObject>();
        Dictionary<int, int> scores = new Dictionary<int, int>();

        int i = 0;
        foreach(KeyValuePair<int, GameObject> entry in players)
        {
            if(i == 0)
            {
                int score = 0;
                foreach(int s in scoreP1)
                {
                    score += s;
                }
                scores.Add(entry.Key, score);
            }
            else if (i == 1)
            {
                int score = 0;
                foreach (int s in scoreP2)
                {
                    score += s;
                }
                scores.Add(entry.Key, score);
            }
            else if (i == 2)
            {
                int score = 0;
                foreach (int s in scoreP3)
                {
                    score += s;
                }
                scores.Add(entry.Key, score);
            }
            else if (i == 3)
            {
                int score = 0;
                foreach (int s in scoreP4)
                {
                    score += s;
                }
                scores.Add(entry.Key, score);
            }
            else
            {
                Debug.LogError("Plus de 4 joueurs ? NANI ?!");
            }


            i++;
        }

        scores.OrderBy(x => x.Value);

        foreach(int key in scores.Keys)
        {
            podium.Add(players[key]);
        }

        return podium;
    }

    public void ShowPlayersScores()
    {
        // Update scores
        int i = 0;
        string[] playersNames = new string[4];

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
            else if (i == 2)
            {
                if (scoreP3.Count < scp.Count)
                {
                    for (int k = scoreP3.Count; k < scp.Count; k++)
                    {
                        scoreP3.Add(scp[k]);
                    }
                }
            }
            else if (i == 3)
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

            playersNames[i] = p.GetComponent<PlayerController>().playerName;
            i++;
        }

        foreach (GameObject o in players.Values)
        {
            o.GetComponent<PlayerController>().ShowScores(playersNames);
        }
    }

    public void TriggerTimeout(int maxShot)
    {
        foreach(GameObject o in players.Values)
        {
            PlayerController pc = o.GetComponent<PlayerController>();
            if (pc.done != true)
            {
                pc.score.Add(maxShot + 2);
                pc.shots = 0;
                pc.SetDone();
            }
        }
    }

    private void TriggerSpawn()
    {
        LobbyManager._instance.SpawnNextPoint();
    }

    public void MovePlayersTo(Transform nextPosition)
    {
        foreach(GameObject o in players.Values)
        {
            PlayerController pc = o.GetComponent<PlayerController>();
            pc.ForcedMoveTo(nextPosition.position);
            pc.EnablePlayer();
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

    public bool isPlayerOnLayerDone(int layer)
    {
        bool res = false;

        foreach (GameObject p in players.Values)
        {
            if (p.layer == layer)
            {
                res = p.GetComponent<PlayerController>().done;
            }
        }

        return res;
    }

    public void SwapPlayers(int target, Vector3 position, uint netid)
    {
        List<int> keys = players.Keys.ToList();
        GameObject targetPlayer = null;

        if (target < keys.Count)
        {
            targetPlayer = players[keys[target]];
        }

        if(targetPlayer != null)
        {
            for(int i = 0;  i < keys.Count; i++)
            {
                GameObject playerOrigin = players[keys[i]];
                NetworkIdentity networkIdentity = playerOrigin.GetComponent<NetworkIdentity>();
                
                if(networkIdentity.netId.Value == netid)
                {
                    Vector3 posOrigin = playerOrigin.transform.position;
                    Vector3 velocityOrigin = playerOrigin.GetComponent<Rigidbody>().velocity;

                    playerOrigin.transform.position = targetPlayer.transform.position;
                    playerOrigin.GetComponent<Rigidbody>().velocity = targetPlayer.GetComponent<Rigidbody>().velocity;
                    targetPlayer.transform.position = posOrigin;
                    targetPlayer.GetComponent<Rigidbody>().velocity = velocityOrigin;

                    targetPlayer.GetComponent<PlayerController>().RpcSetLastPosition(position);

                    //targetPlayer.GetComponent<PlayerController>().RpcSwapPlayer()
                    break;
                }
            }
        }
    }

    public void ChangeGravity(GravityType type, float time, uint netid)
    {
        foreach(GameObject go in players.Values)
        {
            NetworkIdentity networkIdentity = go.GetComponent<NetworkIdentity>();

            if(networkIdentity.netId.Value != netid)
            {
                go.GetComponent<CustomPhysics>().ChangeGravity(type);
            }
        }

        StartCoroutine(WaitForCooldown(time, ResetGravity));
    }

    public void ResetGravity()
    {
        foreach (GameObject go in players.Values)
        {
            go.GetComponent<CustomPhysics>().ResetGravity();
        }
    }

    public void ResetPixelation()
    {
        foreach(GameObject go in players.Values)
        {
            go.GetComponent<PlayerController>().RpcPixelation(false);
        }
    }

    public void Pixelation(float time, uint netid)
    {
        foreach (GameObject go in players.Values)
        {
            NetworkIdentity networkIdentity = go.GetComponent<NetworkIdentity>();

            if(networkIdentity.netId.Value != netid)
            {
                go.GetComponent<PlayerController>().RpcPixelation(true);
            }
        }

        StartCoroutine(WaitForCooldown(time, ResetPixelation));
    }

    public void ChangeSliderSpeed(int sliderSpeed, float time, uint netid)
    {
        foreach (GameObject go in players.Values)
        {
            NetworkIdentity networkIdentity = go.GetComponent<NetworkIdentity>();

            if (networkIdentity.netId.Value != netid)
            {
                go.GetComponent<PlayerController>().RpcChangeSliderSpeed(sliderSpeed);
            }
        }
    }

    public IEnumerator WaitForCooldown(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback.Invoke();
    }
}
