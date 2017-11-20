using System.Collections;
using System.Collections.Generic;

public sealed class PlayerManager {

    private static volatile PlayerManager instance;
    private static object padLock = new object();
    private Dictionary<int, PlayerStatus> players;

    PlayerManager()
    {
        players = new Dictionary<int, PlayerStatus>();
    }

    public static PlayerManager Instance
    {
        get
        {
            if(instance == null)
            {
                lock(padLock)
                {
                    if(instance == null)
                    {
                        instance = new PlayerManager();
                    }
                }
            }

            return instance;
        }
    }
	
    public bool HasPlayer()
    {
        return (players.Count > 0);
    }

    public void AddPlayer(int connId)
    {
        players[connId] = new PlayerStatus();
    }

    public void RemovePlayer(int connId)
    {
        players.Remove(connId);
    }

    public void SetPlayerShots(int connId, int shots)
    {
        players[connId].shots = shots;
    }

    public void SetPlayerDone(int connId)
    {
        players[connId].done = true;
    }

    public void AddPlayerScore(int connId, int shots)
    {
        players[connId].score.Add(shots);
    }

    public bool AllPlayersDone()
    {
        bool allDone = true;

        foreach(PlayerStatus player in players.Values)
        {
            if (!player.done)
                allDone = false;
        }

        return allDone;
    }

    public void ResetAllPlayers()
    {
        foreach (PlayerStatus player in players.Values)
        {
            player.done = false;
            player.shots = 0;
        }
    }

    public void ResetAllPlayersScore()
    {
        foreach(PlayerStatus p in players.Values)
        {
            p.score.Clear();
        }
    }

    public List<List<int>> GetPlayersScore()
    {
        List<List<int>> scores = new List<List<int>>();

        foreach(PlayerStatus p in players.Values)
        {
            scores.Add(p.score);
        }

        return scores;
    }
}
