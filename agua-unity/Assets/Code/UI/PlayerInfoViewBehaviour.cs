using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class PlayerInfoViewBehaviour : MonoBehaviour
    {
        [SerializeField] private TMP_Text _id;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private TMP_Text _phone;
        [SerializeField] private TMP_Text _email;
        [SerializeField] private Button _deleteButton;

        public Player Player { get; private set; }

        public Action<PlayerInfoViewBehaviour> OnPlayerDeleted;

        private bool _deletePlayer;
        
        private void Awake()
        {
            _deleteButton.onClick.AddListener(DeletePlayer);
        }

        private void DeletePlayer()
        {
            AndroidDialog.Show($"Delete {Player.PlayerName}?", "Remove them from all leaderboards?", "Delete", "No", DeletePlayerCallback, () => { });
        }
        
        private void DeletePlayerCallback()
        {
            _deletePlayer = true;
        }

        private void Update()
        {
            if (_deletePlayer)
            {
                LeaderboardState.Provider.DeletePlayer(Player);
                OnPlayerDeleted?.Invoke(this);
                _deletePlayer = false;
            }
        }

        public void SetInfo(Player player, int index)
        {
            _id.text = index.ToString();
            _name.text = player.PlayerName;
            _phone.text = player.PhoneNumber;
            _email.text = player.Email;
            
            Player = player;
        }
    }
}