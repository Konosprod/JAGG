using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameTimer : NetworkBehaviour
{
    [SyncVar] public float timer;
    [SyncVar] public bool masterTimer = false;


    public LevelProperties levelProperties;
    public Text timerText;

    GameTimer serverTimer;

    void Start()
    {
        if (levelProperties == null)
            Debug.Log("NEED LEVEL PROPERTIES");

        timer = levelProperties.maxTime;

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

    void Update()
    {
        if (masterTimer)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                //end of game
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

    void OnGUI()
    {
        timerText.text = timer.ToString("Time: 0 s");
    }
}