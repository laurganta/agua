using System;
using UnityEngine;

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
        
        [Header("Gameplay")]
        [SerializeField] private float _activeSeconds;

        private float _activeTimer;
        private bool _activeDurationEnded;

        public PowerUpType PowerUpType => _powerUpType;
        
        private Action<PowerUpBehaviour> _onActiveDurationEnd;
        private Action<PowerUpBehaviour> _onCollect;

        public void Initialize(Action<PowerUpBehaviour> onCollect,  Action<PowerUpBehaviour> onActiveDurationEnd)
        {
            _onActiveDurationEnd = onActiveDurationEnd;
            _onCollect = onCollect;
        }

        private void OnEnable()
        {
            _activeTimer = 0;
            _activeDurationEnded = false;
        }

        private void Update()
        {
            if (_activeDurationEnded)
                return;
            
            _activeTimer += Time.deltaTime;
            
            if (_activeTimer >= _activeSeconds)
            {
                _onActiveDurationEnd?.Invoke(this);
                _activeDurationEnded = true;
            }
        }

        public void Collect()
        {
            _onCollect?.Invoke(this);
        }
    }
}