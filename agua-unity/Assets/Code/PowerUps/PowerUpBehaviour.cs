using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace CleverEdge
{
    public enum PowerUpType
    {
        ExtraTime,
        Damage,
        Speed,
        ScoreMultiplier,
        Bomb,
        DoubleBarrel,
    }

    public class PowerUpBehaviour : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PowerUpType _powerUpType;
        [SerializeField] private int _spawnLimit;
        [SerializeField] private int _collectionLimit;

        [SerializeField] private bool _isActive;
        
        [Header("Gameplay")]
        [SerializeField] private float _activeSeconds;
        
        [Header("Animations")]
        [SerializeField] private float _showDuration;
        [SerializeField] private AnimationCurve _showCurve;
        [SerializeField] private float _hideDuration;
        [SerializeField] private AnimationCurve _hideCurve;

        private float _activeTimer;
        private bool _activeDurationEnded;

        public PowerUpType PowerUpType => _powerUpType;
        
        private Action<PowerUpBehaviour> _onActiveDurationEnd;
        private Action<PowerUpBehaviour> _onCollect;

        public bool IsActive => _isActive;
        
        public int SpawnLimit => _spawnLimit;
        public int CollectionLimit => _collectionLimit;

        public void Initialize(Action<PowerUpBehaviour> onCollect,  Action<PowerUpBehaviour> onActiveDurationEnd)
        {
            _onActiveDurationEnd = onActiveDurationEnd;
            _onCollect = onCollect;
        }

        private Tweener PlayScaleAnimation(Vector3 startScale, Vector3 targetScale, float duration, AnimationCurve curve)
        {
            transform.localScale = startScale;
            return transform.DOScale(targetScale, duration).SetEase(curve);
        }

        private void OnEnable()
        {
            _activeTimer = 0;
            _activeDurationEnded = false;

            PlayScaleAnimation(Vector3.zero, Vector3.one, _showDuration, _showCurve);
        }

        private void Update()
        {
            if (_activeDurationEnded)
                return;
            
            _activeTimer += Time.deltaTime;
            
            if (_activeTimer >= _activeSeconds)
            {
                _activeDurationEnded = true;
                Hide();
            }
        }

        private void Hide()
        {
            var hideAnimation = PlayScaleAnimation(Vector3.one, Vector3.zero, _hideDuration, _hideCurve);
            hideAnimation.onComplete += () =>
            {
                _onActiveDurationEnd.Invoke(this);
            };
        }

        public void Collect()
        {
            _onCollect?.Invoke(this);
            ServiceLocator.GetInstance<VFXControllerBehaviour>().PlayEffect(VFXEffectType.PowerUpCollect, transform.position, Quaternion.identity);
        }
    }
}