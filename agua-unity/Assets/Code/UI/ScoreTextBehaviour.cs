using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CleverEdge
{
    public class ScoreTextBehaviour : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Vector3 _punchScale;
        [SerializeField] private float _punchDuration;
        [SerializeField] private int _punchVibrato;
        [SerializeField] private Transform _punchTransform;
        
        private Tweener _punchTween;
        
        public void SetValue(float highScore)
        {
            _text.text = highScore.ToString("0");
        }
        
        public void SetValueAnimated(float highScore)
        {
            SetValue(highScore);
            _punchTween?.Kill(true);
            _punchTween = _punchTransform.DOPunchScale(_punchScale, _punchDuration, _punchVibrato);
        }
    }
}
