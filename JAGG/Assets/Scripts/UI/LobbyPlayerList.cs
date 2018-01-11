using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerList : MonoBehaviour
{

    public static LobbyPlayerList _instance = null;

    public RectTransform playerListContentTransform;

    protected List<LobbyPlayer> _players = new List<LobbyPlayer>();
    protected HorizontalLayoutGroup _layout;

    public void OnEnable()
    {
        _instance = this;
        _layout = playerListContentTransform.GetComponent<HorizontalLayoutGroup>();
    }

    void Update()
    {
        //this dirty the layout to force it to recompute evryframe (a sync problem between client/server
        //sometime to child being assigned before layout was enabled/init, leading to broken layouting)

        if (_layout)
            _layout.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
    }


    public void AddPlayer(LobbyPlayer player)
    {
        if (_players.Contains(player))
            return;

        _players.Add(player);

        player.transform.SetParent(playerListContentTransform, false);
    }

    public void RemovePlayer(LobbyPlayer player)
    {
        _players.Remove(player);
    }

}
