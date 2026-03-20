using UnityEngine;

namespace CleverEdge
{
    public class GameplayScreenBehaviour : MonoBehaviour
    {
        private static readonly int ShowHash = Animator.StringToHash("Show");
        
        [SerializeField] private ScoreTextBehaviour _scoreText;
        [SerializeField] private TimerBehaviour _roundTimer;
        [SerializeField] private float _noActivityTutorialDelay;
        [SerializeField] private Animator _tutorialAnimator;

        public int SecondsLeft => _roundTimer.SecondsLeft;
        
        private float _noActivityTimer;
        
        public void SetScore(float score)
        {
            _noActivityTimer = 0;
            _scoreText.SetValue(score);
        }

        public void SetScoreAnimated(float score)
        {
            _noActivityTimer = 0;
            _scoreText.SetValueAnimated(score);
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
        
        private void Update()
        {
            _noActivityTimer += Time.deltaTime;

            if (_noActivityTimer > _noActivityTutorialDelay)
            {
                _tutorialAnimator.SetTrigger(ShowHash);
            }
        }

        public void SetScoreMultiplier(float scoreMultiplier)
        {
            _scoreText.SetScoreMultiplier(scoreMultiplier);
        }

        public void PrepareForRound()
        {
            _scoreText.ResetMultiplier();
        }
    }
}