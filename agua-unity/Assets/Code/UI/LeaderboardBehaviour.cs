using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class LeaderboardBehaviour : MonoBehaviour
    {
        [SerializeField] private LeaderboardEntryBehaviour _leaderboardEntryPrefab;
        [SerializeField] private Transform _entriesParent;
        [SerializeField] private ScrollRect _leaderboardScrollRect;
        [SerializeField] private TMP_Text _leaderboardIndex;

        public Action<string> OnSelectPlayer;
        
        public void Setup()
        {
            LeaderboardState.Provider.OnLoad += Initialize;
        }

        public void Initialize()
        {
            Initialize(LeaderboardState.Provider.Entries, LeaderboardState.Provider.SelectedLeaderboardIndex);
        }

        private void Initialize(List<LeaderboardEntry> entries, int leaderboardIndex)
        {
            _entriesParent.ClearChildren();
            
            _leaderboardIndex.text = $"Match {leaderboardIndex + 1}";
            // gameObject.SetActive(entries.Count > 0);

            if (entries.Count == 0)
                return;


            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                var leaderboardEntryBehaviour = Instantiate(_leaderboardEntryPrefab, _entriesParent);
                leaderboardEntryBehaviour.Set(entry, index);
                leaderboardEntryBehaviour.OnSelectPlayer += OnSelectPlayerClick;
                if (String.CompareOrdinal(entry.Player.PlayerName, LeaderboardState.Provider.LastPlayerName) == 0)
                    leaderboardEntryBehaviour.Highlight(true);
            }
            
            // scroll to highlighted player
            var highlightedPlayerIndex = entries.FindIndex(x => String.CompareOrdinal(x.Player.PlayerName, LeaderboardState.Provider.LastPlayerName) == 0);
            if (highlightedPlayerIndex >= 0)
            {
                var normalizedPosition = 1 - (float)highlightedPlayerIndex / (entries.Count - 1);
                _leaderboardScrollRect.verticalNormalizedPosition = normalizedPosition;
            } else 
                _leaderboardScrollRect.verticalNormalizedPosition = 1;
        }

        private void OnSelectPlayerClick(string playerName)
        {
            OnSelectPlayer?.Invoke(playerName);
        }
    }
}