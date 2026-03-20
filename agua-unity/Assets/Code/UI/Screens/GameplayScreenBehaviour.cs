using UnityEngine;

namespace CleverEdge
{
    public class GameplayScreenBehaviour : MonoBehaviour
    {
        [SerializeField] private ScoreTextBehaviour scoreText;
        [SerializeField] private TimerBehaviour _roundTimer;

        public int SecondsLeft => _roundTimer.SecondsLeft;
        
        public void SetScore(float score)
        {
            scoreText.SetValue(score);
        }

        public void SetScoreAnimated(float score)
        {
            scoreText.SetValueAnimated(score);
        }
        
        public void SetTimeLeft(float timeLeft, bool animated = false)
        {
            if (animated)
            {
                _roundTimer.SetTimeAnimated(timeLeft);
            }
            else
            {
                _roundTimer.SetTime(timeLeft);
            }
        }
    }
}