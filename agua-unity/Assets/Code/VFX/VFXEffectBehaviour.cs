using System;
using UnityEngine;

namespace CleverEdge
{
    public class VFXEffectBehaviour : MonoBehaviour
    {
        [SerializeField] private float _duration;
        [SerializeField] private ParticleSystem _particleSystem;

        private Transform _targetToFollow;
        private bool _shouldStopAfterTargetDies;
        
        private float _timer;

        private Action<VFXEffectBehaviour> _onDispose;

        public VFXEffectType EffectType { get; private set; }

        public void Initialize(Action<VFXEffectBehaviour> onDispose, VFXEffectType type)
        {
            _onDispose = onDispose;
            EffectType = type;
        }
        
        private void Update()
        {
            if (_targetToFollow != null && _targetToFollow.Equals(null) == false && _targetToFollow.gameObject.activeInHierarchy)
            {
                transform.position = _targetToFollow.position;
            } else if (_shouldStopAfterTargetDies)
            {
                _particleSystem.Stop(true);
                
            } else 
            {
                _timer += Time.deltaTime;
                if (_timer >= _duration)
                {
                    Dispose();
                }
            }
        }

        private void Dispose()
        {
            _onDispose?.Invoke(this);
        }

        public void Play(Transform targetToFollow = null)
        {
            _targetToFollow = targetToFollow;
            if (targetToFollow != null)
                _shouldStopAfterTargetDies = true;
            
            _timer = 0;
            _particleSystem.Play(true);
        }
    }
}