using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CleverEdge
{
    public class FlyTextBehaviour : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _duration;
        [SerializeField] private float _moveDistance;
        [SerializeField] private AnimationCurve _fadeCurve;
        
        private Tweener _moveTween;
        private Tweener _fadeTween;
        
        public void SetText(string text, Color color)
        {
            _text.text = text;
            _text.color = color;
        }

        public void PlayAnimation(Action<FlyTextBehaviour> onComplete)
        {
            _text.alpha = 1;
            
            _moveTween?.Kill(true);
            _fadeTween?.Kill(true);
            
            _moveTween = transform.DOLocalMoveY(transform.localPosition.y + _moveDistance, _duration)
                .SetEase(Ease.OutQuint)
                .OnComplete(() =>
                {
                    onComplete?.Invoke(this);
                });
            
            _fadeTween =_text.DOFade(0, _duration).SetEase(_fadeCurve);
        }
    }
}