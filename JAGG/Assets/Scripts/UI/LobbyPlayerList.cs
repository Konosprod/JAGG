using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LobbyPlayerList : MonoBehaviour
{
    public GameObject scrollviewContent;
    public VerticalLayoutGroup _layout;
    public static LobbyPlayerList _instance = null;

    protected List<LobbyPlayer> _players = new List<LobbyPlayer>();

    void OnEnable()
    {
        _instance = this;
    }

    void Update()
    {
        if (_layout)
            _layout.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
    }

    public void AddPlayer(LobbyPlayer player)
    {
        _players.Add(player);
        player.transform.SetParent(scrollviewContent.transform, false);
    }

    public void RemovePlayer(LobbyPlayer player)
    {
        if (_players.Contains(player))
            _players.Remove(player);
    }

    public void RemovePlayerByConnectionID(int conn)
    {
        foreach (LobbyPlayer lp in _players)
        {
            if (lp.GetComponent<NetworkIdentity>().connectionToClient.connectionId == conn)
            {
                _players.Remove(lp);
                break;
            }
        }
    }

    public void ClearPlayers()
    {
        _players.Clear();
    }

    public void UpdateSelectedMap(string levelname)
    {
        foreach(LobbyPlayer lp in _players)
        {
            lp.UpdateSelectedScene(levelname);
        }
    }

    public void UpdateAvatar(ulong steamid)
    {
        foreach(LobbyPlayer lp in _players)
        {
            //if(lp.isLocalPlayer)
                lp.CmdUpdateAvatar(steamid);
        }
    }
}
