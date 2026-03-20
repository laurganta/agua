using System.Collections;
using UnityEngine;

namespace CleverEdge
{
    public class TimerBehaviour : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _timerText;
        [SerializeField] private AnimationCurve _textChangeCurve;
        [SerializeField] private float _textChangeDuration = 0.5f;

        private int _secondsLeft;
        
        private Coroutine _timeCoroutine;

        private Vector3 _originalScale;

        public int SecondsLeft => _secondsLeft; 
        
        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void SetTime(float valueSeconds)
        {
            var minutes = Mathf.FloorToInt(valueSeconds / 60);
            var seconds = Mathf.FloorToInt(valueSeconds % 60);
            
            _timerText.text = $"{minutes:00}:{seconds:00}";
            
            _secondsLeft = minutes * 60 + seconds;
        }

        public void SetTimeAnimated(float valueSeconds)
        {
            SetTime(valueSeconds);
            if (_timeCoroutine != null)
                StopCoroutine(_timeCoroutine);
            
            _timeCoroutine = StartCoroutine(AnimateTextChange());
        }

        private IEnumerator AnimateTextChange()
        {
            var elapsedTime = 0f;

            while (elapsedTime < _textChangeDuration)
            {
                var scaleMultiplier = _textChangeCurve.Evaluate(elapsedTime / _textChangeDuration);
                transform.localScale = _originalScale * scaleMultiplier;


                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localScale = _originalScale;
        }
    }
}