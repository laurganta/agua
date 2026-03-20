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
        ExtraBullet,
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
        [SerializeField] private Transform _scaleRoot;

        [SerializeField] private bool _isActive;

        [Header("Gameplay")] 
        [SerializeField] private Component[] _disableComponentsOnCollect;
        [SerializeField] private float _activeSeconds;
        [SerializeField] private VFXEffectType _collectVFXEffectType;
        
        [Header("Animations")]
        [SerializeField] private float _showDuration;
        [SerializeField] private AnimationCurve _showCurve;
        [SerializeField] private float _hideDuration;
        [SerializeField] private AnimationCurve _hideCurve;

        private float _activeTimer;
        private bool _activeDurationEnded;
        private bool _collected;

        public PowerUpType PowerUpType => _powerUpType;
        
        private Action<PowerUpBehaviour> _onActiveDurationEnd;
        private Action<PowerUpBehaviour> _onCollect;

        public bool IsActive => _isActive;
        
        public int SpawnLimit => _spawnLimit;
        public int CollectionLimit => _collectionLimit;
        
        public Transform ScaleRoot => _scaleRoot;

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
            _collected = false;
            _activeDurationEnded = false;
            _scaleRoot.localScale = Vector3.one;
            SetComponentsActive(true);
            
            PlayScaleAnimation(Vector3.zero, Vector3.one, _showDuration, _showCurve);
        }
        
        private void SetComponentsActive(bool active)
        {
            foreach (var component in _disableComponentsOnCollect)
            {
                if (component is Collider collider)
                    collider.enabled = active;
                else if (component is MonoBehaviour monoBehaviour)
                    monoBehaviour.enabled = active;
            }
        }

        private void Update()
        {
            if (_activeDurationEnded || _collected)
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
            SetComponentsActive(false);
            _onCollect?.Invoke(this);
            _collected = true;

            ServiceLocator.GetInstance<VFXControllerBehaviour>().PlayEffect(_collectVFXEffectType, transform.position, Quaternion.identity);
        }
    }
}