using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class LeaderboardEntryBehaviour : MonoBehaviour
    {
        [Serializable]
        public struct RankSettings
        {
            public int rank;
            public Color scoreColor;
            public int scoreFontSize;
            public GameObject reference;
        }

        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private AvatarBehaviour _avatarBehaviour;
        [SerializeField] private List<RankSettings> _rankSettings;
        [SerializeField] private Button _selectPlayerButton;

        private Player _player;
        
        public Action<string> OnSelectPlayer;
        
        private void Awake()
        {
            _selectPlayerButton.onClick.AddListener(OnSelectButtonClicked);
        }

        private void OnSelectButtonClicked()
        {
            OnSelectPlayer.Invoke(_player.PlayerName);
        }

        public void Set(LeaderboardEntry entry, int rankIndex)
        {
            _player = entry.Player;
            
            _playerNameText.text = entry.Player.PlayerName;
            _scoreText.text = entry.Score.ToString("0");
            _scoreText.color = _rankSettings[rankIndex].scoreColor;
            _scoreText.fontSize = _rankSettings[rankIndex].scoreFontSize;
            
            _avatarBehaviour.SetAvatar(entry.Player.AvatarIndex);

            var rank = rankIndex + 1;
            
            for (int i = 0; i < _rankSettings.Count; i++)
            {
                if (rank >= _rankSettings[i].rank && (i == _rankSettings.Count - 1 || rank < _rankSettings[i + 1].rank))
                {
                    var reference = _rankSettings[i].reference;
                    reference.SetActive(true);
                    reference.GetComponentInChildren<TMP_Text>().text = _rankSettings[i].rank.ToString();
                }
                else
                {
                    _rankSettings[i].reference.SetActive(false);
                }
            }
        }
    }
}