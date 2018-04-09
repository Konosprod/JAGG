using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerList : MonoBehaviour
{

    public static LobbyPlayerList _instance = null;

    public RectTransform playerListContentTransform;

    protected List<LobbyPlayer> _players = new List<LobbyPlayer>();

    public void OnEnable()
    {
        _instance = this;
    }
    

    public void AddPlayer(LobbyPlayer player)
    {
        if (_players.Contains(player))
            return;

        _players.Add(player);
    }

    public void RemovePlayer(LobbyPlayer player)
    {
        _players.Remove(player);
    }

    public void SetLobbyPlayersVisibility(bool visi)
    {
        foreach (LobbyPlayer lp in _players)
            lp.SetVisibility(visi);
    }

}
