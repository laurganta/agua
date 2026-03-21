using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CleverEdge
{
    public class LeaderboardBehaviour : MonoBehaviour
    {
        [SerializeField] private LeaderboardEntryBehaviour _leaderboardEntryPrefab;
        [SerializeField] private Transform _entriesParent;
        [SerializeField] private TMP_Text _leaderboardIndex;

        public Action<string> OnSelectPlayer;
        
        public void Setup()
        {
            LeaderboardState.Provider.OnLoad += Initialize;
        }

        public void Initialize()
        {
            LeaderboardState.Provider.LoadSelectedLeaderboard();
            Initialize(LeaderboardState.Provider.Entries, LeaderboardState.Provider.SelectedLeaderboardIndex);
        }

        private void Initialize(List<LeaderboardEntry> entries, int leaderboardIndex)
        {
            _leaderboardIndex .text = $"Match {leaderboardIndex + 1}";
            gameObject.SetActive(entries.Count > 0);

            if (entries.Count == 0)
                return;

            _entriesParent.ClearChildren();

            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                var leaderboardEntryBehaviour = Instantiate(_leaderboardEntryPrefab, _entriesParent);
                leaderboardEntryBehaviour.Set(entry, index);
                leaderboardEntryBehaviour.OnSelectPlayer += OnSelectPlayerClick;
                if (String.CompareOrdinal(entry.Player.PlayerName, LeaderboardState.Provider.LastPlayerName) == 0)
                    leaderboardEntryBehaviour.Highlight(true);
            }
        }

        private void OnSelectPlayerClick(string playerName)
        {
            OnSelectPlayer?.Invoke(playerName);
        }
    }
}