using System;

namespace CleverEdge
{
    [Serializable]
    public class LeaderboardEntry 
    {
        public Player Player { get; private set; }
        
        public int Score { get; private set; }
        public DateTime Time { get; private set; }

        public LeaderboardEntry(Player player, int score, DateTime time)
        {
            Player = player;
            Score = score;
            Time = time;
        }
        
        public void SetScore(int score, DateTime time)
        {
            Score = score;
            Time = time;
        }
    }
}