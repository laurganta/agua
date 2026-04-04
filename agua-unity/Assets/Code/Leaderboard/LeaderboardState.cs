using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace CleverEdge
{
    public static class LeaderboardState
    {
        public static LeaderboardStateProvider Provider { get; private set; }

        public static void Initialize()
        {
            Provider = new LeaderboardStateProvider();
            Provider.Initialize();
        }
    }

    public class LeaderboardStateProvider
    {
        public Action<List<LeaderboardEntry>, int> OnLoad;
        
        public List<LeaderboardEntry> Entries { get; private set; }

        public string LastPlayerName { get; set; }

        private int LastLeaderboardIndex 
        { 
            get => PlayerPrefs.GetInt("last_leaderboard_index", 0);
            set => PlayerPrefs.SetInt("last_leaderboard_index", value);
        }
        
        public DateTime LastMatchStartTime
        {
            get
            {
                var ticks = PlayerPrefs.GetString("last_round_start_time", "0");
                if (long.TryParse(ticks, out var ticksValue))
                    return new DateTime(ticksValue);

                return DateTime.MinValue;
            }
            set => PlayerPrefs.SetString("last_round_start_time", value.Ticks.ToString());
        }

        public bool HasMatchExpired(int durationMinutes)
        {
            var matchDuration = TimeSpan.FromMinutes(durationMinutes);
            var timeSinceLastMatch = DateTime.Now - LastMatchStartTime;
            return timeSinceLastMatch > matchDuration;
        }

        public int SelectedLeaderboardIndex
        {
            get;
            private set;
        }

        public Player LastPlayer
        {
            get
            {
                if (string.IsNullOrEmpty(LastPlayerName) == false)
                    return Entries
                        .First(x => x.Player.PlayerName == LastPlayerName)
                        .Player;

                return null;
            }
        }

        public void SetNextLeaderboard()
        {
            if (SelectedLeaderboardIndex < LastLeaderboardIndex)
            {
                SelectedLeaderboardIndex++;
                GameDebug.Log($"Next Leaderboard {SelectedLeaderboardIndex}");
                LoadSelectedLeaderboard();
            } 
            else
            {
                GameDebug.Log("Already at the last leaderboard");
            }
        }
        
        public void SetPreviousLeaderboard()
        {
            if (SelectedLeaderboardIndex > 0)
            {
                SelectedLeaderboardIndex--;
                GameDebug.Log($"Previous Leaderboard {SelectedLeaderboardIndex}");
                LoadSelectedLeaderboard();
            }
            else
            {
                GameDebug.Log("Already at the first leaderboard");
            }
        }
        
        public void LoadSelectedLeaderboard()
        {
            Entries = Load(SelectedLeaderboardIndex);
            OnLoad?.Invoke(Entries, SelectedLeaderboardIndex);
        }

        private List<LeaderboardEntry> Load(int leaderboardIndex)
        {
            var filePath = GetFilePath(leaderboardIndex);
            var entries = (List<LeaderboardEntry>) default;
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                entries = JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json);
            }
            else
            {
                entries = new List<LeaderboardEntry>();
            }
            
            entries = Sort(entries);

            GameDebug.Log($"Loaded leaderboard {leaderboardIndex} with {entries.Count} entries");
            
            return entries;
        }

        private List<LeaderboardEntry> Sort(List<LeaderboardEntry> entries)
        {
            return entries.OrderByDescending(e => e.Score).ToList();
        }

        public void SetEntry(LeaderboardEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Player.PlayerName))
            {
                GameDebug.LogWarning($"Can't set leaderboard entry with empty player name");
                return;
            }

            if (string.Equals(entry.Player.PlayerName, Player.Admin.PlayerName))
            {
                GameDebug.LogWarning($"Can't set leaderboard entry for admin player");
                return;
            }

            LastPlayerName = entry.Player.PlayerName;
            
            var scoreSet = false;
            
            Entries = Load(LastLeaderboardIndex);
            
            foreach (var existingEntry in Entries)
                if (string.Equals(existingEntry.Player.PlayerName, entry.Player.PlayerName,
                        StringComparison.InvariantCulture))
                {
                    if (entry.Score < existingEntry.Score)
                        return;

                    existingEntry.SetScore(entry.Score, entry.Time);
                    scoreSet = true;
                    
                    GameDebug.Log($"UPDATE leaderboard entry for player {entry.Player.PlayerName} with new score {entry.Score}");

                    break;
                }

            if (scoreSet == false)
            {
                Entries.Add(entry);
                
                GameDebug.Log($"ADD leaderboard entry for player {entry.Player.PlayerName} with score {entry.Score}");
            }
            
            Entries = Sort(Entries);
            Save(Entries, LastLeaderboardIndex);
            
            SelectedLeaderboardIndex = LastLeaderboardIndex;
        }
        
        private void Save(List<LeaderboardEntry> entries, int leaderboardIndex)
        {
            var json = JsonConvert.SerializeObject(entries, Formatting.Indented);
            File.WriteAllText(GetFilePath(leaderboardIndex), json);
        }
    
        private string GetFilePath(int leaderboardIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"leaderboard_{leaderboardIndex}.json");
        }

        public List<Player> GetAllPlayers()
        {
            // scan all leaderboard files and return all 
            var players = new List<Player>();
            for (var i = 0; i <= LastLeaderboardIndex; i++)
            {
                var entries = Load(i);
                foreach (var entry in entries)
                {
                    if (players.Any(p => p.PlayerName == entry.Player.PlayerName))
                        continue;

                    players.Add(entry.Player);
                }
            }
            
            return players;
        }

        public void DeletePlayer(Player player)
        {
            // loop through all entries and remove the ones matching the player
            // also loop through all files and remove the ones matching the player
            for (var i = 0; i <= LastLeaderboardIndex; i++)
            {
                var entries = Load(i);
                var entriesToRemove = entries.Where(e => e.Player.PlayerName == player.PlayerName).ToList();
                if (entriesToRemove.Count > 0)
                {
                    foreach (var entry in entriesToRemove)
                    {
                        GameDebug.Log($"DELETE leaderboard entry for player {entry.Player.PlayerName} with score {entry.Score}");
                        entries.Remove(entry);
                    }
                    
                    Save(entries, i);
                }
            }
            
            LoadSelectedLeaderboard();
        }

        public void ClearAll()
        {
            // move all files to a backup folder and clear the current entries
            var backupFolder = Path.Combine(Application.persistentDataPath, $"leaderboard_backup_{DateTime.Now:yyyyMMddHHmmss}");
            Directory.CreateDirectory(backupFolder);
            // move files to backup folder
            for (var i = 0; i <= LastLeaderboardIndex; i++)
            {
                var filePath = GetFilePath(i);
                if (File.Exists(filePath))
                {
                    var backupFilePath = Path.Combine(backupFolder, $"leaderboard_{i}.json");
                    File.Move(filePath, backupFilePath);
                }
            }
            
            // clear current entries
            Entries = new List<LeaderboardEntry>();
            LastLeaderboardIndex = 0;
            SelectedLeaderboardIndex = 0;
            OnLoad?.Invoke(Entries, 0);
        }

        public void CreateNewMatch()
        {
            var currentEntries = Load(LastLeaderboardIndex);
            if (currentEntries.Count == 0)
            {
                GameDebug.Log("Current leaderboard is empty, no need to create a new one");
                LastMatchStartTime = DateTime.Now;
                return;
            }

            LastMatchStartTime = DateTime.Now;
            
            LastLeaderboardIndex++;
            SelectedLeaderboardIndex = LastLeaderboardIndex;
            GameDebug.Log($"New Leaderboard {SelectedLeaderboardIndex}");
            Entries.Clear();
            Save(Entries, LastLeaderboardIndex);
            LoadSelectedLeaderboard();
        }

        public void Initialize()
        {
            SelectedLeaderboardIndex = LastLeaderboardIndex;
        }
    }
}