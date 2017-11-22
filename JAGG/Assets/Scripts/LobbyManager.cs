using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine;
using UnityEngine.UI;


public class LobbyManager : NetworkLobbyManager
{
    [Header("Game Logic")]
    public Transform EndOfGamePos;
    public PlayerManager playerManager;

    [Header("UI")]
    public InputField InputIP;

    private GameObject hole;
    private int currentHole = 1;

    private GameTimer gameTimer;
    private GameObject[] balls;


    private bool gotAllPlayer = false;
    private bool isStarted = false;

    private const int FirstLayer = 9;
    private bool[] layers = new bool[4];

    // Use this for initialization
    void Start()
    {
        if (playerManager == null)
            Debug.Log("need playermanager");

        if (EndOfGamePos == null)
            Debug.Log("NEED ENDOFGAMEPREFAB");
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
        {
            if (!gotAllPlayer)
            {
                balls = GameObject.FindGameObjectsWithTag("Player");
                Debug.Log(balls.Length);
                for (int i = 0; i < balls.Length; i++)
                {
                    Debug.Log("Trying to add player");
                    playerManager.AddPlayer(balls[i]);
                    Debug.Log("Added player");
                }
                gotAllPlayer = true;

                playerManager.ui = GameObject.FindObjectOfType<UIManager>();
            }
            else
            {
                if (GameObject.FindGameObjectsWithTag("Player").Length != balls.Length)
                {
                    gotAllPlayer = false;
                    playerManager.ClearPlayers();
                }
            }
        }
    }

    public void TriggerTimeout()
    {
        //Show recap here
        SpawnNextPoint();
    }

    public void SpawnNextPoint()
    {
        Transform nextPosition = hole.GetComponentInChildren<LevelProperties>().nextSpawnPoint;

        if (nextPosition.position != EndOfGamePos.position)
        {
            disableAllBallsCollisions();

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
            EndOfGame();
        }

    }

    public int GetPar()
    {
        return hole.GetComponentInChildren<LevelProperties>().par;
    }

    private void EndOfGame()
    {
        playerManager.isStarted = false;
        isStarted = false;
        gameTimer.StopTimer();
        playerManager.ResetAllPlayers();
        playerManager.ResetAllPlayersScore();
        currentHole = 1;
        gotAllPlayer = false;
        layers = new bool[4];

        this.SendReturnToLobby();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        base.OnLobbyServerSceneChanged(sceneName);

        if (sceneName != "Lobby")
        {
            playerManager.isStarted = true;
            isStarted = true;
            gameTimer = GameObject.Find("GameTimer").GetComponent<GameTimer>();

            gameTimer.StartTimer(hole.GetComponentInChildren<LevelProperties>().maxTime);

        }
        else
        {
            //disable mainpanel, only return to lobby
            GameObject.Find("PanelMain").SetActive(false);
        }
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        hole = GameObject.Find("Hole " + currentHole.ToString());
        gamePlayer.layer = getNextLayer();
        return base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        return base.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        playerManager.RemovePlayer(conn.connectionId);
        base.OnClientDisconnect(conn);
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


    private int getNextLayer()
    {
        int l;
        for (l = FirstLayer; layers[l-FirstLayer] && l-FirstLayer<4; l++) ;

        if (l - FirstLayer == 3 && layers[l - FirstLayer])
            Debug.LogError("layers not reset");

        layers[l-FirstLayer] = true;
        return l;
    }

    private void disableAllBallsCollisions()
    {
        Physics.IgnoreLayerCollision(FirstLayer, FirstLayer + 1, true);
        Physics.IgnoreLayerCollision(FirstLayer, FirstLayer + 2, true);
        Physics.IgnoreLayerCollision(FirstLayer, FirstLayer + 3, true);
        Physics.IgnoreLayerCollision(FirstLayer + 1, FirstLayer + 2, true);
        Physics.IgnoreLayerCollision(FirstLayer + 1, FirstLayer + 3, true);
        Physics.IgnoreLayerCollision(FirstLayer + 2, FirstLayer + 3, true);
    }
}