using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CleverEdge
{
    public class PlayerInfoListBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform _viewParent;
        [SerializeField] private PlayerInfoViewBehaviour _viewPrefab;
        [SerializeField] private TMP_InputField _searchField;

        private readonly List<PlayerInfoViewBehaviour> _views = new List<PlayerInfoViewBehaviour>();

        private void Awake()
        {
            _searchField.onValueChanged.AddListener(Search);
        }

        private void OnEnable()
        {
            var players = LeaderboardState.Provider.GetAllPlayers();
            
            SetPlayerInfos(players);
        }

        private void Search(string searchString)
        {
            for (var i = 0; i < _views.Count; i++)
            {
                var view = _views[i];
                var playerName = view.Player.PlayerName;
                
                view.gameObject.SetActive(string.IsNullOrEmpty(searchString) || playerName.ToLower().Contains(searchString.ToLower()));
            }
        }

        private void SetPlayerInfos(List<Player> players)
        {
            _viewParent.ClearChildren();
            _views.Clear();

            for (var i = 0; i < players.Count; i++)
            {
                var playerInfo = players[i];
                var view = Instantiate(_viewPrefab, _viewParent);
                view.OnPlayerDeleted += OnPlayerDeleted;
                view.SetInfo(playerInfo, i + 1);
                
                _views.Add(view);
            }
        }

        private void OnPlayerDeleted(PlayerInfoViewBehaviour playerView)
        {
            playerView.gameObject.SetActive(false);
        }

        public void SearchPlayer(string playerName)
        {
            _searchField.text = playerName;
        }
    }
}