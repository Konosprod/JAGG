using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkLobbyManager
{
    static public LobbyManager _instance;

    [Header("Game Logic")]
    public Transform EndOfGamePos;
    public PlayerManager playerManager;

    [Header("UI")]
    public GameObject mainPanel;
    public PanelJoin joinPanel;
    public GameObject lobbyPanel;
    public GameObject controlPanel;
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

    // Use this for initialization
    void Start()
    {
        _instance = this;

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
            if (!setUi)
            {
                playerManager.ui = GameObject.FindObjectOfType<UIManager>();
                setUi = true;
            }
        }
    }

    public void ReturnToLobby()
    {
        SceneManager.LoadScene("Lobby");
        mainPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
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
        return ruleSet.holes[currentHole-1].properties.maxShot;
    }

    public float GetMaxTime()
    {
        return ruleSet.holes[currentHole-1].properties.maxTime;
    }

    private void EndOfGame()
    {
        playerManager.isStarted = false;
        isStarted = false;
        gameTimer.StopTimer();
        currentHole = 1;
        setUi = false;
        layers = new bool[4];

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SendReturnToLobby();
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

        if (sceneName != lobbyScene)
        {
            playerManager.isStarted = true;
            isStarted = true;
            gameTimer = GameObject.Find("GameTimer").GetComponent<GameTimer>();

            gameTimer.StartTimer(GetMaxTime());

        }
        else
        {
            mainPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            joinPanel.SetActive(false);
            controlPanel.SetActive(true);
        }
    }

    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        base.OnLobbyClientSceneChanged(conn);

        if (SceneManager.GetSceneAt(0).name == lobbyScene)
        {
            lobbyPanel.SetActive(true);
            controlPanel.SetActive(true);
            
            for(int i = 0; i < lobbySlots.Length; i++)
            {
                if (lobbySlots[i] != null)
                    (lobbySlots[i] as LobbyPlayer).ResetStatus();
            }

            playerManager.ClearPlayers();
        }
        else
        {
            mainPanel.SetActive(false);
            lobbyPanel.SetActive(false);
            joinPanel.SetActive(false);
            controlPanel.SetActive(false);

        }
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        hole = GameObject.Find("Hole " + currentHole.ToString());

        playerManager.AddPlayer(gamePlayer, lobbyPlayer.GetComponent<NetworkIdentity>().connectionToClient.connectionId);
        gamePlayer.layer = getNextLayer();

        return base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
    }

    public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
    {
        return base.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
    }

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject o = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;

        return o;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        playerManager.RemovePlayer(conn.connectionId);
        base.OnServerDisconnect(conn);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        mainPanel.SetActive(false);
        joinPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        controlPanel.SetActive(true);
        base.OnClientConnect(conn);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log(conn);

        if (conn.lastError == NetworkError.Ok)
        {
            playerManager.RemovePlayer(conn.connectionId);
        }
        else
        {
            joinPanel.Error();
        }

        mainPanel.SetActive(false);
        joinPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        controlPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        base.OnClientDisconnect(conn);
    }

    public override void OnStopHost()
    {
        playerManager.ClearPlayers();
        base.OnStopHost();
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