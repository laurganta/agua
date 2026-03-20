using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace CleverEdge
{
    public class GunBehaviour : MonoBehaviour
    {
        [Header("Bullet")] 
        [SerializeField] private BulletBehaviour[] _bulletPrefabs;
        [SerializeField] private Transform _bulletParent;
        [SerializeField] private Transform _bulletDirection;
        [SerializeField] private Transform _bulletSpawnPoint;
        [SerializeField] private int _poolSize;
        [SerializeField] private float _shootRate;
        [SerializeField] private float _shootRandomAngle;
        [SerializeField] private float _extraBulletAngleMultiplier;
        [SerializeField] private float _scaleRandom;

        [Header("Gun")] 
        [SerializeField] private Transform _gunRoot;
        [SerializeField] private Vector3 _gunPunchScale;
        [SerializeField] private float _gunPunchDuration;
        [SerializeField] private int _gunPunchVibrato;
        [SerializeField] private float _gunPunchRotationAngle;
        [SerializeField] private ParticleSystem _shootParticleSystem;
        [SerializeField] private GameObject _extraBulletVisual;
        
        private Dictionary<BulletType, ObjectPool<BulletBehaviour>> _bulletPools;
        
        private float _shootTimer;
        private float _shootTimerExtra;
        
        private bool _shooting;
        
        private Tweener _gunShootTween;
        private Tweener _gShootRotationTween;

        private VFXControllerBehaviour _vfxController;

        private int _currentBulletIndex;
        private int _maxBulletIndex;
        private float _shootRandomAngleMultiplier;

        private void Awake()
        {
            InitializePool();

            _vfxController = ServiceLocator.GetInstance<VFXControllerBehaviour>();
        }

        public void PrepareForRound()
        {
            _currentBulletIndex = 0;
            _maxBulletIndex = 0;
            _shootRandomAngleMultiplier = 1;
            _extraBulletVisual.SetActive(false);
        }
        
        public void IncreaseMaxBulletIndex()
        {
            _shootRandomAngleMultiplier = _extraBulletAngleMultiplier;
            _extraBulletVisual.SetActive(true);
            _maxBulletIndex = Mathf.Min(_maxBulletIndex + 1, _bulletPools.Count - 1);
        }

        private void NextBulletIndex()
        {
            _currentBulletIndex++;
            if (_currentBulletIndex > _maxBulletIndex)
                _currentBulletIndex = 0;
        }

        private void InitializePool()
        {
            _bulletPools = new Dictionary<BulletType, ObjectPool<BulletBehaviour>>();
            
            foreach (var bulletPrefab in _bulletPrefabs)
            {
                var pool = new ObjectPool<BulletBehaviour>(
                    createFunc: () =>
                    {
                        var bullet = Instantiate(bulletPrefab, _bulletSpawnPoint.position, Quaternion.identity);
                        bullet.transform.SetParent(_bulletParent);
                        bullet.transform.forward = _bulletDirection.forward;
                        var bulletBehaviour = bullet.GetComponent<BulletBehaviour>();
                        bulletBehaviour.Initialize(_vfxController, OnBulletDestroy);
                        return bulletBehaviour;
                    },
                    actionOnGet: (bullet) =>
                    {
                        bullet.transform.position = _bulletSpawnPoint.position;
                        bullet.transform.rotation = Quaternion.identity;
                        bullet.transform.localScale = Vector3.one + Vector3.one * Random.Range(-_scaleRandom, _scaleRandom);
                    
                        // set bullet random forward with angle to player forward
                        var randomAngleMultiplier = _shootRandomAngleMultiplier * _shootRandomAngle;
                        var angle = Random.Range(-randomAngleMultiplier, randomAngleMultiplier);
                        var rotation = Quaternion.Euler(0, angle, 0);
                        bullet.transform.forward = rotation * _bulletDirection.forward;
                    
                        bullet.gameObject.SetActive(true);
                    },
                    actionOnRelease: (bullet) =>
                    {
                        bullet.gameObject.SetActive(false);
                    },
                    actionOnDestroy: (bullet) =>
                    {
                        if (bullet)
                            Destroy(bullet.gameObject);
                    },
                    collectionCheck: false,
                    defaultCapacity: 10,
                    maxSize: 100
                );
                
                _bulletPools.Add(bulletPrefab.BulletType, pool);
            }
        }

        private void OnBulletDestroy(BulletBehaviour bullet)
        {
            foreach (var (poolType, pool) in _bulletPools)
            {
                if (bullet.BulletType == poolType)
                {
                    pool.Release(bullet);
                    return;
                }
            }
            
            GameDebug.LogWarning($"No pool found for bullet type {bullet.BulletType}");
        }
        
        private BulletBehaviour GetNextBullet()
        {
            var bulletType = _bulletPrefabs[_currentBulletIndex].BulletType;
            
            if (_bulletPools.TryGetValue(bulletType, out var pool))
            {
                var bullet = pool.Get();
                NextBulletIndex();

                return bullet;
            }
            else
            {
                GameDebug.LogWarning($"No pool found for bullet type {bulletType}");
                return null;
            }
        }

        public void Shoot()
        {
            _gunShootTween?.Kill(true);
            _gShootRotationTween?.Kill(true);

            _gunShootTween = _gunRoot.DOPunchScale(_gunPunchScale, _gunPunchDuration, _gunPunchVibrato);
            _gShootRotationTween = _gunRoot.DOShakeRotation(_gunPunchDuration, Vector3.right * -_gunPunchRotationAngle, _gunPunchVibrato);

            var bullet = GetNextBullet();
            _vfxController.PlayEffect(bullet.VFXEffectType, bullet.transform.position, Quaternion.identity, bullet.transform);
            
            _shootParticleSystem.Play();
            
        }
    }
}