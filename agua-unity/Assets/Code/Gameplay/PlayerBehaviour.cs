using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
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

        [SerializeField] private GunBehaviour _rightGunBehaviour;
        [SerializeField] private GunBehaviour _leftGunBehaviour;
        
        [SerializeField] private float _fixedAimZ;
        [SerializeField] private Transform _actorRoot;
        [SerializeField] private Vector3 _actorPunchScale;
        [SerializeField] private float _actorPunchDuration;
        [SerializeField] private int _actorPunchVibrato;
        
        [Header("Can")]
        [SerializeField] private Transform _canRoot;
        [SerializeField] private float _canShakeAngle;
        [SerializeField] private float _canShakeDuration;
        [SerializeField] private int _canPunchVibrato;

        [SerializeField] private float _shootRate;
        
        private InputSystem_Actions _inputSystemActions;
        
        private float _shootTimer;
        private bool _shooting;
        
        private Tweener _actorShootTween;
        private Tweener _canPunchTween;

        private void Awake()
        {
            ServiceLocator.AddInstance(this);
            
            _inputSystemActions = ServiceLocator.GetInstance<InputSystem_Actions>();
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
        
        public void PrepareForRound()
        {
            _shooting = false;
            _shootTimer = 0;
            SetLeftHandActive(false);
        }

        public void SetLeftHandActive(bool active)
        {
            _leftGunBehaviour.gameObject.SetActive(active);
        }

        private void Shoot()
        {
            _actorShootTween?.Kill(true);
            _canPunchTween?.Kill(true);

            _actorShootTween = _actorRoot.DOPunchPosition(_actorPunchScale, _actorPunchDuration, _actorPunchVibrato);
            _canPunchTween = _canRoot.DOShakeRotation(_canShakeDuration, Vector3.one * -_canShakeAngle, _canPunchVibrato);

            if (_rightGunBehaviour.isActiveAndEnabled)
                _rightGunBehaviour.Shoot();
            
            if (_leftGunBehaviour.isActiveAndEnabled)
                _leftGunBehaviour.Shoot();
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