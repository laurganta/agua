using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace CleverEdge
{
    public class RegisterStateBehaviour : GameStateBehaviourBase
    {
        [SerializeField] private RegisterScreenBehaviour _registerScreenBehaviour;
        [SerializeField] private float _idleTimeoutSeconds = 30f;
        
        public Action OnRegister;
        public Action OnBackButton;
        public Action OnGDPRButton;
        
        private Player _playerCache;
        private float _currentScore;

        private float _idleTimer;

        private void Awake()
        {
            _registerScreenBehaviour.OnRegister += OnRegisterClick;
            _registerScreenBehaviour.OnBackButton += OnBackButtonClick;
            _registerScreenBehaviour.OnGDPRButton += OnGdprButtonClick;
            _registerScreenBehaviour.OnActivity += OnActivity;
        }

        private void OnActivity()
        {
            _idleTimer = 0;
        }

        private void OnGdprButtonClick()
        {
            _playerCache = GetPlayer();
            
            OnGDPRButton?.Invoke();
        }

        public void SetCurrentScore(float currentScore)
        {
            _currentScore = currentScore;
        }

        private void OnRegisterClick()
        {
            var player = GetPlayer();

            Player.Current = player;
            
            LeaderboardState.Provider.SetEntry(
                new LeaderboardEntry(Player.Current,
                    Mathf.FloorToInt(_currentScore), DateTime.Now));

            OnRegister.Invoke();
        }

        private Player GetPlayer()
        {
            var playerName = _registerScreenBehaviour.PlayerName;
            var phone = _registerScreenBehaviour.Phone;
            var email = _registerScreenBehaviour.Email;
            var avatarIndex = _registerScreenBehaviour.AvatarIndex;
            var gdprAccepted = _registerScreenBehaviour.GdprAccepted;
            
            GameDebug.Log($"Registering user: Name={playerName}, Phone={phone}, Email={email}, Avatar={avatarIndex}, GDPR Accepted={gdprAccepted}");

            var player = new Player()
            {
                PlayerName = playerName,
                PhoneNumber = phone,
                Email = email,
                AvatarIndex = avatarIndex,
            };
            return player;
        }

        private void OnBackButtonClick()
        {
            OnBackButton.Invoke();
        }

        private void OnEnable()
        {
            _registerScreenBehaviour.gameObject.SetActive(true);
            _registerScreenBehaviour.SetRandomAvatar();

            _registerScreenBehaviour.PrepareForRegister((int) _currentScore);

            if (_playerCache != null)
            {
                _registerScreenBehaviour.SetPlayer(_playerCache);
                _playerCache = null;
            }

            LeaderboardState.Provider.LastPlayerName = null;
        }
        
        private void OnDisable()
        {
            if (_registerScreenBehaviour)
                _registerScreenBehaviour.gameObject.SetActive(false);
        }

        public void SetGDPRAccepted()
        {
            _registerScreenBehaviour.SetGDPRAccepted();
        }

        public void SetPlayer(Player player)
        {
            _registerScreenBehaviour.SetPlayer(player);
        }

        private void Update()
        {
            if (Touchscreen.current.primaryTouch.phase.value == TouchPhase.Began)
                _idleTimer = 0;
            
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _idleTimeoutSeconds)
            {
                _idleTimer = 0;
                OnBackButton.Invoke();
            }
        }
    }
}