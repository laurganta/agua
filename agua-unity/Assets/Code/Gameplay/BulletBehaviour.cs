using System;
using UnityEngine;

namespace CleverEdge
{
    public enum BulletType
    {
        Default,
        Extra,
    }

    public class BulletBehaviour : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private BulletType _type;
        [SerializeField] private float _speed;
        [SerializeField] private float _lifetimeSeconds;
        [SerializeField] private float _damage;
        [SerializeField] private VFXEffectType _vfxEffectType;

        [Header("Collision")]
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private float _hitDistance;
        [SerializeField] private float _raycastRadius;
        
        private Action<BulletBehaviour> _onDestroy;
        private float _lifetimeTimer;

        private VFXControllerBehaviour _vfxController;
        
        public BulletType BulletType => _type;
        public VFXEffectType VFXEffectType => _vfxEffectType;

        private void OnEnable()
        {
            _lifetimeTimer = 0f;
        }

        public void Initialize(VFXControllerBehaviour vfxControllerBehaviour, Action<BulletBehaviour> onDestroy)
        {
            _vfxController = vfxControllerBehaviour;
            _onDestroy = onDestroy;
        }
        
        private void Update()
        {
            transform.position += transform.forward * (_speed * Time.deltaTime);
            
            _lifetimeTimer += Time.deltaTime;
            
            if (_lifetimeTimer >= _lifetimeSeconds)
                _onDestroy?.Invoke(this);
        }

        private void FixedUpdate()
        {
            Debug.DrawLine(transform.position, transform.position + transform.forward * _hitDistance, Color.magenta);
            var hit = Physics.SphereCast(transform.position - transform.forward * (_hitDistance * 0.5f), _raycastRadius, transform.forward, out var hitInfo, _hitDistance, _hitMask, QueryTriggerInteraction.Collide);
            if (hit)
            {
                var enemy = hitInfo.collider.gameObject.GetComponentInParent<EnemyBehaviour>(true);
                if (enemy)
                {
                    enemy.Damage(_damage, hitInfo.point);
                }
                else
                {
                    var powerUp = hitInfo.collider.gameObject.GetComponentInParent<PowerUpBehaviour>(true);
                    if (powerUp)
                    {
                        powerUp.Collect();
                    }
                }
                
                _onDestroy?.Invoke(this);
                _vfxController.PlayEffect(VFXEffectType.BulletHit, hitInfo.point, Quaternion.identity);
            }
        }
    }
}