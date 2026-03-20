using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CleverEdge
{
    public class ScoreTextBehaviour : MonoBehaviour
    {
        [Header("Score")]
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Vector3 _punchScale;
        [SerializeField] private float _punchDuration;
        [SerializeField] private int _punchVibrato;
        [SerializeField] private Transform _punchTransform;
        
        [Header("Multiplier")]
        [SerializeField] private TMP_Text _multiplierText;
        [SerializeField] private float _multiplierPunchDuration;
        [SerializeField] private float _multiplierPunchScale;
        [SerializeField] private int _multiplierPunchVibrato;
        
        private Tweener _punchTween;

        private void Awake()
        {
            _multiplierText.gameObject.SetActive(false);
        }

        public void SetValue(float highScore)
        {
            _text.text = highScore.ToString("0");
        }
        
        public void SetValueAnimated(float highScore)
        {
            SetValue(highScore);
            _punchTween?.Kill(true);
            _punchTween = _punchTransform.DOPunchScale(_punchScale, _punchDuration, _punchVibrato).SetEase(Ease.InCubic);
        }

        public void SetScoreMultiplier(float multiplier)
        {
            _multiplierText.gameObject.SetActive(true);
            _multiplierText.text = $"x{multiplier:0}";

            _multiplierText.transform.DOPunchScale(Vector3.one * _multiplierPunchScale, _multiplierPunchDuration, _multiplierPunchVibrato);
        }
        
        public void ResetMultiplier()
        {
            _multiplierText.gameObject.SetActive(false);
        }
    }
}
