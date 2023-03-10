using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


#pragma warning disable CS0618 // Le type ou le membre est obsolète

public class PlayerManager : NetworkBehaviour
{
    [SyncVar]
    public int currentHole = 1;

    public UIManager ui;
    private Dictionary<int, GameObject> players;
    //Client loading statuses
    private Dictionary<int, bool> states;

    private SyncListInt scoreP1 = new SyncListInt();
    private SyncListInt scoreP2 = new SyncListInt();
    private SyncListInt scoreP3 = new SyncListInt();
    private SyncListInt scoreP4 = new SyncListInt();

    private SyncListString playersNames = new SyncListString();

    public static PlayerManager _instance;

    [SyncVar]
    private int nbPlayers = 0;


    [HideInInspector]
    public bool isStarted;



    // For sending replay
    // Client only stuff
    private int receivedSize = 0;
    private int replaySize = 0;
    private byte[] clientData;

    // Server only
    private int replayReceived = 0;

    [HideInInspector]
    public bool isHighlight = false;
    private bool highlightStarted = false;


    void Awake()
    {
        if (_instance == null)
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
        states = new Dictionary<int, bool>();

        LobbyManager._instance.client.RegisterHandler(MyMsgType.ReplayInfo, OnReplayInfo);
        LobbyManager._instance.client.RegisterHandler(MyMsgType.ReplayChunk, OnReplayChunk);
        LobbyManager._instance.client.RegisterHandler(MyMsgType.StartHighlights, OnStartHighlights);
        NetworkServer.RegisterHandler(MyMsgType.ReplayDone, OnReplayDone);
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

        if (isServer && isHighlight && !highlightStarted)
        {
            if (players.Count - 1 == replayReceived)
            {
                highlightStarted = true;
                ReplayManager._instance.StartHighlights();

                // The balls must be deactivated so that they don't show in the replay
                foreach (GameObject o in players.Values)
                {
                    o.SetActive(false);
                }

                // Tell the clients to start the highlights as well
                foreach (int clientId in players.Keys)
                {
                    if (clientId != 0) // clientId 0 on server is the server itself
                    {
                        NetworkServer.SendToClient(clientId, MyMsgType.StartHighlights, new StartHighlights());
                    }
                }
            }
        }
    }

    public List<GameObject> GetPodium()
    {
        List<GameObject> podium = new List<GameObject>();
        Dictionary<int, int> scores = new Dictionary<int, int>();

        int i = 0;
        foreach (KeyValuePair<int, GameObject> entry in players)
        {
            if (i == 0)
            {
                int score = 0;
                foreach (int s in scoreP1)
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

        foreach (int key in scores.Keys)
        {
            podium.Add(players[key]);
        }

        return podium;
    }

    public SyncListString GetPlayerNames()
    {
        return playersNames;
    }

    public void ShowPlayersScores()
    {
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

            i++;
        }

        foreach (GameObject o in players.Values)
        {
            o.GetComponent<PlayerController>().ShowScores();
        }
    }

    public void TriggerTimeout(int maxShot)
    {
        foreach (GameObject o in players.Values)
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
        foreach (GameObject o in players.Values)
        {
            PlayerController pc = o.GetComponent<PlayerController>();
            pc.replayObj.StartHoleReplay(currentHole);
            pc.RpcResetCameraTarget();
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
        states.Clear();
        players.Clear();
        scoreP1.Clear();
        scoreP2.Clear();
        scoreP3.Clear();
        scoreP4.Clear();
        playersNames.Clear();
        nbPlayers = 0;
    }

    public void AddPlayer(GameObject o, int connId = -1)
    {
        if (connId == -1)
            connId = o.GetComponent<NetworkIdentity>().connectionToClient.connectionId;

        players[connId] = o;
        PlayerController pc = o.GetComponent<PlayerController>();
        string name = pc.playerName;
        playersNames.Add(name);

        pc.replayObj.SetupReplay(o);
        pc.replayObj.steamName = pc.playerName;
        ReplayManager._instance.AddReplayObject(pc.replayObj);

        nbPlayers++;
    }

    public void RemovePlayer(int connId)
    {
        if (players.Keys.Contains(connId))
        {
            GameObject o = players[connId];
            players.Remove(connId);
            playersNames.Remove(o.GetComponent<PlayerController>().playerName);
        }
    }

    public void StartGame()
    {
        isHighlight = false;
        highlightStarted = false;

        foreach (GameObject player in players.Values)
        {
            player.GetComponent<PlayerController>().RpcStartGame();
        }
    }

    public void SetPlayerReady(int connId = -1)
    {
        states[connId] = true;

        if (AllPlayerLoaded())
        {
            StartGame();
        }
    }

    public bool AllPlayerLoaded()
    {
        if (nbPlayers == players.Count())
        {
            foreach (bool state in states.Values)
            {
                if (!state)
                    return false;
            }

            return true;
        }

        return false;
    }

    public bool AllPlayersDone()
    {
        bool allDone = true;

        foreach (GameObject player in players.Values)
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
        foreach (GameObject p in players.Values)
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

    public bool IsPlayerOnLayerDone(int layer)
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

        if (targetPlayer != null)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                GameObject playerOrigin = players[keys[i]];
                NetworkIdentity networkIdentity = playerOrigin.GetComponent<NetworkIdentity>();

                if (networkIdentity.netId.Value == netid)
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

    // Spectate another player
    public void GetSpectate(uint netid)
    {
        GameObject ball = null;
        GameObject spectate = null;

        List<GameObject> checkSpectates = new List<GameObject>();

        if (!AllPlayersDone())
        {
            foreach (GameObject go in players.Values)
            {
                NetworkIdentity networkIdentity = go.GetComponent<NetworkIdentity>();

                if (networkIdentity.netId.Value == netid)
                {
                    ball = go;
                }
                else if (!go.GetComponent<PlayerController>().done)
                {
                    spectate = go;
                }
                else
                {
                    checkSpectates.Add(go);
                }
            }
        }

        if (ball == null)
        {
            Debug.LogError("Couldn't find the ball with netid : " + netid + ", so no spectating :(");
            return;
        }

        // Check if the finished players need to spectate someone else now that netid has finished the hole
        foreach (GameObject spec in checkSpectates)
        {
            spec.GetComponent<PlayerController>().RpcCheckSpectate(ball);
        }

        if (spectate != null)
        {
            ball.GetComponent<PlayerController>().RpcChangeSpectate(spectate);
        }
    }
    // Change spectate to another player
    public void ChangeSpectate(uint netid, GameObject currSpectate)
    {
        GameObject ball = null;
        GameObject spectate = null;

        List<GameObject> potentialSpectates = new List<GameObject>();

        if (!AllPlayersDone())
        {
            foreach (GameObject go in players.Values)
            {
                NetworkIdentity networkIdentity = go.GetComponent<NetworkIdentity>();

                if (networkIdentity.netId.Value == netid)
                {
                    ball = go;
                }
                else if (!go.GetComponent<PlayerController>().done)
                {
                    potentialSpectates.Add(go);
                }
            }
        }

        int i = potentialSpectates.IndexOf(currSpectate);
        if (i != -1)
        {
            spectate = potentialSpectates[(i + 1) % potentialSpectates.Count];
        }
        else
        {
            Debug.LogError("The player is currently spectating a player that does not exist");
        }

        if (spectate != null)
        {
            if (ball != null)
            {
                ball.GetComponent<PlayerController>().RpcChangeSpectate(spectate);
            }
            else
                Debug.LogError("Couldn't find the ball with netid : " + netid + ", so no spectating :(");
        }
    }


    public void ChangeGravity(GravityType type, float time, uint netid)
    {
        foreach (GameObject go in players.Values)
        {
            NetworkIdentity networkIdentity = go.GetComponent<NetworkIdentity>();

            if (networkIdentity.netId.Value != netid)
            {
                go.GetComponent<BallPhysicsNetwork>().ChangeGravity(type);
            }
        }

        StartCoroutine(WaitForCooldown(time, ResetGravity));
    }

    public void ResetGravity()
    {
        foreach (GameObject go in players.Values)
        {
            go.GetComponent<BallPhysicsNetwork>().ResetGravity();
        }
    }

    public void ResetInvertedCamera()
    {
        foreach (GameObject go in players.Values)
        {
            go.GetComponent<PlayerController>().RpcInvertCamera(false);
        }
    }

    public void InvertCamera(float time, uint netid)
    {
        foreach (GameObject go in players.Values)
        {
            NetworkIdentity networkIdentity = go.GetComponent<NetworkIdentity>();

            if (networkIdentity.netId.Value != netid)
            {
                go.GetComponent<PlayerController>().RpcInvertCamera(true);
            }
        }

        StartCoroutine(WaitForCooldown(time, ResetPixelation));
    }

    public void ResetPixelation()
    {
        foreach (GameObject go in players.Values)
        {
            go.GetComponent<PlayerController>().RpcPixelation(false);
        }
    }

    public void Pixelation(float time, uint netid)
    {
        foreach (GameObject go in players.Values)
        {
            NetworkIdentity networkIdentity = go.GetComponent<NetworkIdentity>();

            if (networkIdentity.netId.Value != netid)
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


    public void AddPlayersScoresReplay()
    {
        foreach (GameObject o in players.Values)
        {
            PlayerController pc = o.GetComponent<PlayerController>();
            int i = 0;
            foreach (int s in pc.score)
            {
                //Debug.Log("AddScoreToReplay : " + pc.playerName + " hole " + (i+1) + " : " + s);
                pc.replayObj.scores[i] = s;
                i++;
            }

            // The game is over so the current longest shot is the longest shot of the game (of that player)
            ReplayManager._instance.AddHighlight(ReplayManager.HighlightType.LongestShotOnTarget, pc.replayObj);
        }
    }




    public class MyMsgType
    {
        public static short ReplayInfo = MsgType.Highest + 1;
        public static short ReplayChunk = MsgType.Highest + 2;
        public static short ReplayDone = MsgType.Highest + 3;
        public static short StartHighlights = MsgType.Highest + 4;
    };
    public class ReplayInfo : MessageBase
    {
        public int replaySize;
    }
    public class ReplayChunk : MessageBase
    {
        public byte[] replayData;
    }
    public class ReplayDone : MessageBase
    {

    }
    public class StartHighlights : MessageBase
    {

    }

    [Server]
    public void SendReplayToClients()
    {
        byte[] data = ReplayManager._instance.GetReplayData();
        foreach (int clientId in players.Keys)
        {
            if (clientId != 0) // clientId 0 on server is the server itself
            {
                Debug.Log("Send replay to clientId : " + clientId + ", replay size : " + data.Length);

                ReplayInfo msg = new ReplayInfo();
                msg.replaySize = data.Length;
                NetworkServer.SendToClient(clientId, MyMsgType.ReplayInfo, msg);

                StartCoroutine(SendReplayRoutine(data, clientId));
            }
        }
    }

    [Server]
    IEnumerator SendReplayRoutine(byte[] data, int clientId)
    {
        int bufferSize = 1024;
        int dataLeft = data.Length;

        while (0 < dataLeft)
        {
            int remaining = dataLeft - bufferSize;
            if (remaining < 0)
            {
                bufferSize = dataLeft;
            }

            byte[] buffer = new byte[bufferSize];
            System.Array.Copy(data, data.Length - dataLeft, buffer, 0, bufferSize);

            //send the chunk
            ReplayChunk msg = new ReplayChunk();
            msg.replayData = buffer;
            NetworkServer.SendToClient(clientId, MyMsgType.ReplayChunk, msg);
            dataLeft -= bufferSize;

            yield return null;
        }
    }

    // Receive the first message that gives the size of the array
    public void OnReplayInfo(NetworkMessage netMsg)
    {
        ReplayInfo msg = netMsg.ReadMessage<ReplayInfo>();
        replaySize = msg.replaySize;
        Debug.Log("Received replay size : " + replaySize);
        clientData = new byte[replaySize];
    }

    // Receive the chunks of replay and add them to the array
    // When everything is received create the replay and save it in a file
    public void OnReplayChunk(NetworkMessage netMsg)
    {
        ReplayChunk msg = netMsg.ReadMessage<ReplayChunk>();
        byte[] data = msg.replayData;

        System.Array.Copy(data, 0, clientData, receivedSize, data.Length);
        receivedSize += data.Length;

        Debug.Log("Received size : " + receivedSize + ", replaySize : " + replaySize);

        if (replaySize == receivedSize)
        {
            Debug.Log("Received everything");
            ReplayManager._instance.GetReplayFromData(clientData, replaySize);
            ReplayManager._instance.SaveInFile();

            LobbyManager._instance.client.Send(MyMsgType.ReplayDone, new ReplayDone());
        }
    }

    public void OnReplayDone(NetworkMessage netMsg)
    {
        Debug.Log("OnReplayDone");
        replayReceived++;
    }

    public void OnStartHighlights(NetworkMessage netMsg)
    {
        Debug.Log("OnStartHighlight");

        // The balls must be deactivated so that they don't show in the replay
        foreach (GameObject o in players.Values)
        {
            o.SetActive(false);
        }

        ReplayManager._instance.StartHighlights();        
    }
}
