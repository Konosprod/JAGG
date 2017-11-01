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

    [Header("Game Logic")]
    public Transform EndOfGamePos;

    [Header("UI")]
    public InputField InputIP;

    [HideInInspector]
    public List<PlayerStatus> players;
    [HideInInspector]
    public bool isStarted = false;

    private bool allDone = true;
    private GameObject hole;
    private int currentHole = 1;

    private GameTimer gameTimer;

    // Use this for initialization
    void Start()
    {
        players = new List<PlayerStatus>();

        if (EndOfGamePos == null)
            Debug.Log("NEED ENDOFGAMEPREFAB");
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
        {
            if (players.Count <= 0)
                return;

            for (int i = 0; i < players.Count; i++)
            {
                if (!players[i].done)
                    allDone = false;
            }

            if (allDone)
            {
                SpawnNextPoint();
            }

            allDone = true;
        }
    }

    public void TriggerTimeout()
    {
        //Show recap here
        SpawnNextPoint();
    }

    private void SpawnNextPoint()
    {
        Transform nextPosition = hole.GetComponentInChildren<LevelProperties>().nextSpawnPoint;

        if (nextPosition.position != EndOfGamePos.position)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].done = false;
            }

            GameObject[] balls = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < balls.Length; i++)
            {
                balls[i].transform.position = nextPosition.position;
                balls[i].GetComponent<PlayerController>().EnablePlayer();
            }

            currentHole++;
            hole = GameObject.Find("Hole " + currentHole.ToString());

            gameTimer.StartTimer(hole.GetComponentInChildren<LevelProperties>().maxTime);
        }
        else
        {
            isStarted = false;
            gameTimer.StopTimer();

            Debug.Log("End of game, return to lobby in 3sec");
            StartCoroutine(WaitAndReturnToLobby());
        }

    }

    IEnumerator WaitAndReturnToLobby()
    {
        yield return new WaitForSeconds(3.0f);
        this.SendReturnToLobby();
    }

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        if (sceneName != "Lobby")
        {
            isStarted = true;
            gameTimer = GameObject.Find("GameTimer").GetComponent<GameTimer>();

            gameTimer.StartTimer(hole.GetComponentInChildren<LevelProperties>().maxTime);

        }
        else
        {
            //disable mainpanel, only return to lobby
            GameObject.Find("PanelMain").SetActive(false);
        }
        base.OnLobbyServerSceneChanged(sceneName);
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        hole = GameObject.Find("Hole " + currentHole.ToString());

        return base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
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
        Debug.Log("here");
        this.StartClient();
    }
}