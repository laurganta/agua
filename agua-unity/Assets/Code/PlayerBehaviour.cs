using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace CleverEdge
{
    public class PlayerBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerType _playerType;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Camera _aimCamera;
        [SerializeField] private Transform _root;
        [SerializeField] private VFXControllerBehaviour _vfxControllerBehaviour;
        
        [Header("Bullet")]
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Transform _bulletSpawnPoint;
        [SerializeField] private int _poolSize;
        [SerializeField] private float _shootRate;
        [SerializeField] private float _shootRandomAngle;
        [SerializeField] private float _scaleRandom;

        [Header("Gun")] 
        [SerializeField] private float _fixedAimZ;
        [SerializeField] private Transform _actorRoot;
        [SerializeField] private Vector3 _actorPunchScale;
        [SerializeField] private float _actorPunchDuration;
        [SerializeField] private int _actorPunchVibrato;
        [SerializeField] private Transform _gunRoot;
        [SerializeField] private Vector3 _gunPunchScale;
        [SerializeField] private float _gunPunchDuration;
        [SerializeField] private int _gunPunchVibrato;
        [SerializeField] private float _gunPunchRotationAngle;
        [SerializeField] private ParticleSystem _shoortParticleSystem;
        
        [Header("Can")]
        [SerializeField] private Transform _canRoot;
        [SerializeField] private float _canShakeAngle;
        [SerializeField] private float _canShakeDuration;
        [SerializeField] private int _canPunchVibrato;
        
        private InputSystem_Actions _inputSystemActions;
        
        private ObjectPool<BulletBehaviour> _bulletPool;
        private float _shootTimer;
        private bool _shooting;
        
        private Tweener _gunShootTween;
        private Tweener _gShootRotationTween;
        private Tweener _actorShootTween;
        private Tweener _canPunchTween;

        private void Awake()
        {
            _inputSystemActions = ServiceLocator.GetInstance<InputSystem_Actions>();

            InitializePool();
        }

        private void InitializePool()
        {
            _bulletPool = new ObjectPool<BulletBehaviour>(
                createFunc: () =>
                {
                    var bullet = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, Quaternion.identity);
                    bullet.transform.SetParent(transform);
                    bullet.transform.forward = transform.forward;
                    var bulletBehaviour = bullet.GetComponent<BulletBehaviour>();
                    bulletBehaviour.Initialize(_vfxControllerBehaviour, OnBulletDestroy);
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
                    bullet.transform.forward = rotation * _root.forward;
                    
                    bullet.gameObject.SetActive(true);
                },
                actionOnRelease: (bullet) =>
                {
                    bullet.gameObject.SetActive(false);
                },
                actionOnDestroy: (bullet) => { Destroy(bullet.gameObject); },
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 100
            );
        }

        private void OnBulletDestroy(BulletBehaviour bullet)
        {
            _bulletPool.Release(bullet);
        }

        private void OnEnable()
        {
            switch (_playerType)
            {
                case PlayerType.Player1:
                    _inputSystemActions.Player1.Enable();
                    _inputSystemActions.Player1.Shoot.started += StartShoot;
                    _inputSystemActions.Player1.Shoot.canceled += StopShoot;
                    _inputSystemActions.Player1.Special.performed += PerformSpecial;
                    break;
                case PlayerType.Player2:
                    _inputSystemActions.Player2.Enable();
                    _inputSystemActions.Player2.Shoot.started += StartShoot;
                    _inputSystemActions.Player2.Shoot.canceled += StopShoot;
                    _inputSystemActions.Player2.Special.performed += PerformSpecial;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void OnDisable()
        {
            switch (_playerType)
            {
                case PlayerType.Player1:
                    _inputSystemActions.Player1.Disable();
                    _inputSystemActions.Player1.Shoot.started -= StartShoot;
                    _inputSystemActions.Player1.Shoot.canceled -= StopShoot;
                    _inputSystemActions.Player1.Special.performed -=  PerformSpecial;
                    break;
                case PlayerType.Player2:
                    _inputSystemActions.Player2.Disable();
                    _inputSystemActions.Player2.Shoot.started -= StartShoot;
                    _inputSystemActions.Player2.Shoot.canceled -= StopShoot;
                    _inputSystemActions.Player2.Special.performed -= PerformSpecial;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Shoot()
        {
            _actorShootTween?.Kill(true);
            _gunShootTween?.Kill(true);
            _gShootRotationTween?.Kill(true);
            _canPunchTween?.Kill(true);

            _actorShootTween = _actorRoot.DOPunchPosition(_actorPunchScale, _actorPunchDuration, _actorPunchVibrato);
            _gunShootTween = _gunRoot.DOPunchScale(_gunPunchScale, _gunPunchDuration, _gunPunchVibrato);
            _gShootRotationTween = _gunRoot.DOShakeRotation(_gunPunchDuration, Vector3.right * -_gunPunchRotationAngle, _gunPunchVibrato);
            _canPunchTween = _canRoot.DOShakeRotation(_canShakeDuration, Vector3.one * -_canShakeAngle, _canPunchVibrato);
            
            var bullet = _bulletPool.Get();
            _vfxControllerBehaviour.PlayEffect(VFXEffectType.BulletBubbles, bullet.transform.position, Quaternion.identity, bullet.transform);
            
            _shoortParticleSystem.Play();
        }

        private void StartShoot(InputAction.CallbackContext obj)
        {
            GameDebug.Log($"Start shoot {_playerType}");
            _shooting = true;
            Shoot();

        }
        
        private void StopShoot(InputAction.CallbackContext obj)
        {
            GameDebug.Log($"Stop shoot {_playerType}");
            _shooting = false;
        }
        
        private void PerformSpecial(InputAction.CallbackContext ctx)
        {
            GameDebug.Log($"Special {_playerType}");
        }

        private void Aim(Vector3 direction)
        {
            var euler = _root.rotation.eulerAngles;
            var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            _root.rotation = Quaternion.Euler(euler.x, angle, euler.z);
        }

        private Vector3 GetAimDirection()
        {
            var mousePosition = Touchscreen.current.primaryTouch.position.ReadValue();
            var ray = _aimCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, _layerMask))
            {
                Debug.DrawLine(hit.point, transform.position, Color.blue);
                
                var hitPoint = hit.point;
                hitPoint.z = _fixedAimZ;
                
                var direction= hitPoint - _root.position;
                return direction;
            }

            return Vector3.zero;
        }

        private void Update()
        {
            var direction = GetAimDirection();
            Aim(direction);

            if (_shooting)
            {
                _shootTimer += Time.deltaTime;
                if (_shootTimer >= _shootRate)
                {
                    Shoot();
                    _shootTimer = 0;
                }
            }
            else
            {
            }
        }
    }
}