using System;
using DG.Tweening;
using UnityEngine;

namespace CleverEdge
{
    public class EnemyBehaviour : MonoBehaviour
    {
        private static readonly int DieHash = Animator.StringToHash("Die");

        [Header("References")]
        [SerializeField] private Transform _inOutAnimations;
        [SerializeField] private Transform _damageRoot;
        [SerializeField] private Transform _centerRoot;
        [SerializeField] private Animator _animatorController;
        [SerializeField] private EnemyMovementBehaviour _movementBehaviour;

        [Header("Configuration")]
        [SerializeField] private float _startHealth;

        [Header("Show Animation")] 
        [SerializeField] private float _showAnimationDuration;
        
        [Header("Death Animation")] 
        [SerializeField] private AnimationCurve _deathAnimationCurve;
        [SerializeField] private float _deathAnimationDuration;
        [SerializeField] private float _deathAnimationScale;
        [SerializeField] private float _deathAnimationRotation;
        
        [Header("Hide Animation")]
        [SerializeField] private AnimationCurve _hideAnimationCurve;
        [SerializeField] private float _hideAnimationDuration;
        
        [Header("Damage Animation")] 
        [SerializeField] private Vector3 _punchScale;
        [SerializeField] private float _punchDuration;
        [SerializeField] private int _punchVibrato;
        
        private float _currentHealth;
        private Quaternion _startRotation;
        private Tweener _damagePunchTweener;
        
        private Action<EnemyBehaviour, bool> _onDeath;

        private VFXControllerBehaviour _vfxController;
        
        public EnemyTier Tier { get; private set; }

        public void Initialize(EnemyTier tier, 
            Action<EnemyBehaviour, bool> onDeath,
            VFXControllerBehaviour vfxController)
        {
            _onDeath = onDeath;
            Tier = tier;
            _vfxController = vfxController;
            
            _startRotation = _inOutAnimations.localRotation;
            
            Reset();
        }

        public void Reset()
        {
            DoShowAnimation();
            _currentHealth = _startHealth;   
            
            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (var childCollider in colliders)
                childCollider.enabled = true;
        }

        private void Die(bool wasKilled)
        {
            Handheld.Vibrate();
            _movementBehaviour?.Stop();
            
            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (var childCollider in colliders)
                childCollider.enabled = false;

            if (wasKilled)
            {
                var deathAnimation = PlayDeathAnimation();

                deathAnimation.onComplete += () =>
                {
                    var vfxType = Tier.ToExplosionEffectType();
                    _vfxController.PlayEffect(vfxType, _centerRoot.position, Quaternion.identity);

                    GameDebug.Log($"{Time.frameCount} Death animation completed, invoking death callback");
                    _onDeath?.Invoke(this, true);
                };
            }
            else
            {
                _inOutAnimations.DOScale(Vector3.zero, _hideAnimationDuration).SetEase(_hideAnimationCurve).onComplete += () =>
                {
                    GameDebug.Log($"{Time.frameCount} Hide animation completed, invoking death callback");
                    _onDeath?.Invoke(this, false);
                };
            }
        }

        public void Damage(float damage, Vector3 hitPosition)
        {
            _currentHealth -= damage;

            if (_currentHealth <= 0f)
            {
                Die(true);
            }
            else
            {
                PlayDamageAnimation(hitPosition);
            }
        }
        
        private void PlayDamageAnimation(Vector3 hitPosition)
        {
            var hitEffect = Tier.ToHitEffectType();
            _vfxController.PlayEffect(hitEffect, hitPosition, Quaternion.identity);
            
            _damagePunchTweener?.Kill(true);
            _damagePunchTweener = _damageRoot.DOPunchScale(_punchScale, _punchDuration, _punchVibrato);
        }

        private Tweener PlayDeathAnimation()
        {
            _animatorController.SetTrigger(DieHash);
            _inOutAnimations.DOLocalRotate(Vector3.right * _deathAnimationRotation, _deathAnimationDuration).SetEase(_deathAnimationCurve);
            GameDebug.Log($"{Time.frameCount} Playing death animation with duration {_deathAnimationDuration}");
            return _inOutAnimations.DOScale(Vector3.one * _deathAnimationScale, _deathAnimationDuration).SetEase(_deathAnimationCurve);
        }

        private Tweener DoShowAnimation()
        {
            _inOutAnimations.localRotation = _startRotation;
            _inOutAnimations.localScale = Vector3.zero;
            return _inOutAnimations.DOScale(Vector3.one, _showAnimationDuration).SetEase(Ease.OutCubic);
        }

        public Vector3 SetMovementPath(EnemyMovementPathBehaviour path)
        {
            return _movementBehaviour.SetPath(path, () =>
            {
                Die(false);
            });
        }
        
        public void HideAfterSeconds(float seconds)
        {
            _movementBehaviour.HideAfterSeconds(seconds, () => Die(false));
        }

        public EnemyMovementPathBehaviour GetMovementPath()
        {
            return _movementBehaviour.CurrentPath;
        }
    }
}