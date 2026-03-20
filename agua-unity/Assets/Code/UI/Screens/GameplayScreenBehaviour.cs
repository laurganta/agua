using System.Collections;
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
        [SerializeField] private Animator _readySetGoAnimator;

        public int SecondsLeft => _roundTimer.SecondsLeft;
        public bool IsPlayingInto => _isPlayingIntro;

        private float _noActivityTimer;

        private bool _isPlaying;
        private bool _isPlayingIntro;

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
            if (_isPlaying == false)
                return;
            
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
            _isPlaying = false;
            _isPlayingIntro = false;
            _readySetGoAnimator.gameObject.SetActive(false);
        }

        public void StartPlaying()
        {
            _isPlaying = true;
        }

        public void PlayTutorialFinger()
        {
            _tutorialAnimator.gameObject.SetActive(true);
            _tutorialAnimator.SetTrigger(ShowHash);
        }

        public void PlayIntro()
        {
            StartCoroutine(PlayIntroCoroutine());
        }

        private IEnumerator PlayIntroCoroutine()
        {
            _isPlayingIntro = true;

            yield return new WaitForSeconds(_tutorialAnimator.GetCurrentAnimatorStateInfo(0).length);
            
            _readySetGoAnimator.gameObject.SetActive(true);
         
            var animator = _readySetGoAnimator.GetComponent<Animator>();
            animator.Play("ReadySetGo", 0, 0);

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            _isPlayingIntro = false;
        }
    }
}