using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManager : NetworkLobbyManager
{
    static public LobbyManager _instance;

    [Header("Game Logic")]
    public Transform EndOfGamePos;
    public PlayerManager playerManager;
    public MovingPieceManager mvpManager;

    [Header("UI")]
    public GameObject mainPanel;
    public GameObject controlPanel;
    public PanelJoin joinPanel;
    public InputField InputIP;
    public InputField joinPort;
    public InputField createPort;

    [Header("Custom Map")]
    public string customMapFile = "";

    [HideInInspector]
    public GameObject hole;

    private int currentHole = 1;

    private GameTimer gameTimer;

    private bool setUi = false;
    private bool isStarted = false;

    private const int FirstLayer = 9;
    private bool[] layers = new bool[4];
    public CustomLevel ruleSet;
    
    [HideInInspector]
    public string winnerName = "";

    private static Vector3[] positions = new Vector3[] {
        new Vector3(-320, 0, 220),
        new Vector3(-118, 0, 220),
        new Vector3(97, 0, 220),
        new Vector3(315, 0, 220)
    };

    //0 = normal, 1 = low, 2 = high
    public int gravity;

    // Use this for initialization
    void Start()
    {
        _instance = this;

        if (playerManager == null)
            Debug.Log("need playermanager");

        if (EndOfGamePos == null)
            Debug.Log("NEED ENDOFGAMEPREFAB");

        mainPanel.GetComponent<Fader>().FadeIn(0.2f, delegate() { mainPanel.GetComponent<CanvasGroup>().blocksRaycasts = true; });
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
        {
            if (!setUi)
            {
                playerManager.ui = GameObject.FindObjectOfType<UIManager>();
                setUi = true;
            }
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!isStarted)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ReturnToLobby()
    {
        MovingPieceManager._instance.ClearRotatePieces();
        MovingPieceManager._instance.ClearMovingPieces();
        SceneManager.LoadScene("Lobby");
        mainPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void TriggerTimeout()
    {
        playerManager.TriggerTimeout(GetMaxShot());
        //playerManager.ShowPlayersScores();
        //SpawnNextPoint();
    }

    public void SpawnNextPoint()
    {
        Transform nextPosition = hole.GetComponentInChildren<LevelProperties>().nextSpawnPoint;

        if (nextPosition.position != EndOfGamePos.position)
        {
            disableAllBallsCollisions();

            playerManager.MovePlayersTo(nextPosition);

            currentHole++;
            hole = GameObject.Find("Hole " + currentHole.ToString());

            gameTimer.StartTimer(GetMaxTime());
        }
        else
        {
            EndOfGame();
        }

    }

    public void StopTimer()
    {
        gameTimer.StopTimer();
    }

    public int GetPar()
    {
        return hole.GetComponentInChildren<LevelProperties>().par;
    }

    public int GetMaxShot()
    {
        if (ruleSet.holes.Count == 0)
            return hole.GetComponentInChildren<LevelProperties>().maxShot;
        else
            return ruleSet.holes[currentHole-1].properties.maxShot;
    }

    public float GetMaxTime()
    {
        if (ruleSet.holes != null && ruleSet.holes.Count == 0)
            return hole.GetComponentInChildren<LevelProperties>().maxTime;
        else
            return ruleSet.holes[currentHole-1].properties.maxTime;
    }

    private void EndOfGame()
    {
        MovingPieceManager._instance.ClearRotatePieces();
        MovingPieceManager._instance.ClearMovingPieces();
        playerManager.isStarted = false;
        isStarted = false;
        gameTimer.StopTimer();
        currentHole = 1;
        setUi = false;
        layers = new bool[4];

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        NetworkManager.singleton.ServerChangeScene("Victory");

        Invoke("SendReturnToLobby", 10);
    }

    private IEnumerator WaitBeforeExec(float time, Func<bool> callback = null)
    {
        yield return new WaitForSeconds(time);

        if (callback != null)
        {
            playerManager.ClearPlayers();
            callback();
        }
    }

    public override void OnLobbyServerSceneChanged(string sceneName)
    {
        base.OnLobbyServerSceneChanged(sceneName);

        if (sceneName != lobbyScene && sceneName != "Victory")
        {
            playerManager.isStarted = true;
            isStarted = true;
            gameTimer = GameObject.Find("GameTimer").GetComponent<GameTimer>();

            gameTimer.StartTimer(GetMaxTime());

            MovingPieceManager._instance.GrabAllRotatePieces();
            MovingPieceManager._instance.GrabAllMovingPieces();

        }
        else if(sceneName == lobbyScene)
        {
            isStarted = false;

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

            mainPanel.SetActive(false);
            LobbyPlayerList._instance.SetLobbyPlayersVisibility(true);
            controlPanel.SetActive(true);
        }
        else
        {
            // Victory Scene

            List<GameObject> podium = playerManager.GetPodium();

            GameObject first = GameObject.Find("1st");
            GameObject second = GameObject.Find("2nd");
            GameObject third = GameObject.Find("3rd");

            first.GetComponent<VictoryBall>().playerName = podium[0].GetComponent<PlayerController>().playerName;

            second.SetActive(false);
            third.SetActive(false);

            if(podium.Count >= 2)
            {
                second.GetComponent<VictoryBall>().playerName = podium[1].GetComponent<PlayerController>().playerName;
                second.SetActive(true);
                third.SetActive(false);
            }

            if(podium.Count >= 3)
            {
                third.GetComponent<VictoryBall>().playerName = podium[2].GetComponent<PlayerController>().playerName;
                third.SetActive(true);
            }

            //Disable controls
            for (int i = 0; i < podium.Count; i++)
            {
                podium[i].GetComponent<PlayerController>().enabled = false;
            }

            mainPanel.SetActive(false);
            LobbyPlayerList._instance.SetLobbyPlayersVisibility(false);
            controlPanel.SetActive(false);
        }
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        base.OnLobbyClientSceneChanged(conn);

        if (SceneManager.GetSceneAt(0).name == lobbyScene)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            
            LobbyPlayerList._instance.SetLobbyPlayersVisibility(true);
            controlPanel.SetActive(true);
            
            for(int i = 0; i < lobbySlots.Length; i++)
            {
                if (lobbySlots[i] != null)
                    (lobbySlots[i] as LobbyPlayer).ResetStatus();
                else
                    LobbyPlayerList._instance.RemovePlayer(lobbySlots[i] as LobbyPlayer);
            }

            playerManager.ClearPlayers();
        }
        else if(SceneManager.GetSceneAt(0).name == "Victory")
        {
            mainPanel.SetActive(false);
            LobbyPlayerList._instance.SetLobbyPlayersVisibility(false);
            controlPanel.SetActive(false);
        }
        else
        {
            isStarted = false;
            mainPanel.SetActive(false);
            LobbyPlayerList._instance.SetLobbyPlayersVisibility(false);
            controlPanel.SetActive(false);
        }
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        hole = GameObject.Find("Hole " + currentHole.ToString());

        playerManager.AddPlayer(gamePlayer, lobbyPlayer.GetComponent<NetworkIdentity>().connectionToClient.connectionId);
        gamePlayer.layer = getNextLayer();
        gamePlayer.GetComponent<PlayerController>().playerName = lobbyPlayer.GetComponent<LobbyPlayer>().playerName;


        return base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        return base.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
    }

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject o = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;
        o.GetComponent<LobbyPlayer>().position = positions[numPlayers];

        return o;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        playerManager.RemovePlayer(conn.connectionId);
        LobbyPlayerList._instance.RemovePlayerByConnectionID(conn.connectionId);
        base.OnServerDisconnect(conn);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        mainPanel.SetActive(false);
        LobbyPlayerList._instance.SetLobbyPlayersVisibility(true);
        controlPanel.SetActive(true);
        base.OnClientConnect(conn);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        if (conn.lastError == NetworkError.Ok)
        {
            playerManager.RemovePlayer(conn.connectionId);
        }
        else
        {
            joinPanel.Error();
        }

        mainPanel.SetActive(true);
        LobbyPlayerList._instance.SetLobbyPlayersVisibility(false);
        controlPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        base.OnClientDisconnect(conn);
    }

    public override void OnStopHost()
    {
        playerManager.ClearPlayers();
        LobbyPlayerList._instance.ClearPlayers();
        base.OnStopHost();
    }

    public override void OnStopClient()
    {
        playerManager.ClearPlayers();
        LobbyPlayerList._instance.ClearPlayers();
        base.OnStopClient();
    }

    public void CreateRoom()
    {
        this.networkPort = 33333;
        try
        {
            this.networkPort = createPort.text == "" ? this.networkPort : int.Parse(createPort.text);
        }
        catch (FormatException)
        {
            Debug.LogError("Format exception");
        }
        catch (OverflowException)
        {
            Debug.LogError("Overflow exception");
        }
        this.StartHost();
    }

    public void JoinRoom()
    {
        this.networkAddress = InputIP.text;
        this.networkPort = 33333;
        try
        { 
            this.networkPort = joinPort.text == "" ? this.networkPort : int.Parse(joinPort.text);
        }
        catch (FormatException)
        {
            Debug.LogError("Format exception");
        }
        catch (OverflowException)
        {
            Debug.LogError("Overflow exception");
        }
        this.StartClient();

        joinPanel.Connecting();
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