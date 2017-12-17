using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerList : MonoBehaviour {

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

            player.transform.SetParent(playerListContentTransform, false);
        }

        public void RemovePlayer(LobbyPlayer player)
        {
            _players.Remove(player);
        }

}
