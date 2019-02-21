using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


#pragma warning disable CS0618 // Le type ou le membre est obsolète

public class GameTimer : NetworkBehaviour
{
    [SyncVar] public float timer;
    [SyncVar] public bool masterTimer = false;

    public Text timerText;

    private bool isStarted = false;

    private GameTimer serverTimer;
    private LobbyManager lobbyManager;

    void Start()
    {
        lobbyManager = GameObject.FindObjectOfType<LobbyManager>();
    }

    void Update()
    {
        if (isStarted)
        {
            if (masterTimer)
            {
                timer -= Time.deltaTime;

                if (timer <= 0)
                {
                    isStarted = false;
                    timer = 0;
                    lobbyManager.TriggerTimeout();
                    //Next Point
                }
            }

            if (isLocalPlayer)
            {
                if (serverTimer)
                {
                    timer = serverTimer.timer;
                }
                else
                {
                    GameTimer[] timers = FindObjectsOfType<GameTimer>();
                    for (int i = 0; i < timers.Length; i++)
                    {
                        if (timers[i].masterTimer)
                        {
                            serverTimer = timers[i];
                        }
                    }
                }
            }
        }
    }

    public void StartTimer(float timer)
    {
        isStarted = true;

        this.timer = timer;

        if (isServer)
        {
            if (isLocalPlayer)
            {
                serverTimer = this;
                masterTimer = true;
            }
        }
        else if (isLocalPlayer)
        {
            GameTimer[] timers = FindObjectsOfType<GameTimer>();
            for (int i = 0; i < timers.Length; i++)
            {
                if (timers[i].masterTimer)
                {
                    serverTimer = timers[i];
                }
            }
        }
    }

    public void StopTimer()
    {
        isStarted = false;

        timer = 0;

        GameTimer[] timers = FindObjectsOfType<GameTimer>();
        for (int i = 0; i < timers.Length; i++)
        {
            if (timers[i].masterTimer)
            {
                serverTimer = timers[i];
            }
        }
    }

    void OnGUI()
    {
        timerText.text = timer.ToString("Time: 0 s");
    }
}