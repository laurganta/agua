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
        [SerializeField] private Transform _bulletParent;
        [SerializeField] private Transform _bulletDirection;
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Transform _bulletSpawnPoint;
        [SerializeField] private int _poolSize;
        [SerializeField] private float _shootRate;
        [SerializeField] private float _shootRandomAngle;
        [SerializeField] private float _scaleRandom;

        [Header("Gun")] 
        [SerializeField] private Transform _gunRoot;
        [SerializeField] private Vector3 _gunPunchScale;
        [SerializeField] private float _gunPunchDuration;
        [SerializeField] private int _gunPunchVibrato;
        [SerializeField] private float _gunPunchRotationAngle;
        [SerializeField] private ParticleSystem _shootParticleSystem;
        
        private ObjectPool<BulletBehaviour> _bulletPool;
        private float _shootTimer;
        private bool _shooting;
        
        private Tweener _gunShootTween;
        private Tweener _gShootRotationTween;

        private VFXControllerBehaviour _vfxController;

        private void Awake()
        {
            InitializePool();

            _vfxController = ServiceLocator.GetInstance<VFXControllerBehaviour>();
        }

        private void InitializePool()
        {
            _bulletPool = new ObjectPool<BulletBehaviour>(
                createFunc: () =>
                {
                    var bullet = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, Quaternion.identity);
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
                    var angle = Random.Range(-_shootRandomAngle, _shootRandomAngle);
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
        }

        private void OnBulletDestroy(BulletBehaviour bullet)
        {
            _bulletPool.Release(bullet);
        }

        public void Shoot()
        {
            _gunShootTween?.Kill(true);
            _gShootRotationTween?.Kill(true);

            _gunShootTween = _gunRoot.DOPunchScale(_gunPunchScale, _gunPunchDuration, _gunPunchVibrato);
            _gShootRotationTween = _gunRoot.DOShakeRotation(_gunPunchDuration, Vector3.right * -_gunPunchRotationAngle, _gunPunchVibrato);
            
            var bullet = _bulletPool.Get();
            _vfxController.PlayEffect(VFXEffectType.BulletBubbles, bullet.transform.position, Quaternion.identity, bullet.transform);
            
            _shootParticleSystem.Play();
        }
    }
}