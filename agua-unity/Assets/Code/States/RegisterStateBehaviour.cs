using System;
using System.Linq;
using UnityEngine;

namespace CleverEdge
{
    public class RegisterStateBehaviour : GameStateBehaviourBase
    {
        [SerializeField] private RegisterScreenBehaviour _registerScreenBehaviour;
        
        public Action OnRegister;
        public Action OnBackButton;
        public Action OnGDPRButton;
        
        private void Awake()
        {
            _registerScreenBehaviour.OnRegister += OnRegisterClick;
            _registerScreenBehaviour.OnBackButton += OnBackButtonClick;
            _registerScreenBehaviour.OnGDPRButton += OnGdprButtonClick;
        }

        private void OnGdprButtonClick()
        {
            OnGDPRButton?.Invoke();
        }

        private void OnRegisterClick()
        {
            var playerName = _registerScreenBehaviour.PlayerName;
            var phone = _registerScreenBehaviour.Phone;
            var email = _registerScreenBehaviour.Email;
            var avatarIndex = _registerScreenBehaviour.AvatarIndex;
            var gdprAccepted = _registerScreenBehaviour.GdprAccepted;
            
            GameDebug.Log($"Registering user: Name={playerName}, Phone={phone}, Email={email}, Avatar={avatarIndex}, GDPR Accepted={gdprAccepted}");

            Player.Current = new Player()
            {
                PlayerName = playerName,
                PhoneNumber = phone,
                Email = email,
                AvatarIndex = avatarIndex,
            };
            
            OnRegister.Invoke();
        }
        
        private void OnBackButtonClick()
        {
            OnBackButton.Invoke();
        }

        private void OnEnable()
        {
            _registerScreenBehaviour.gameObject.SetActive(true);
            _registerScreenBehaviour.SetRandomAvatar();

            // var entry = LeaderboardState.Provider.Entries.OrderBy(x => x.Time).FirstOrDefault();
            //
            // if (entry != null)
            //     _registerScreenBehaviour.SetPlayer(entry.Player);
            // else 
            //     _registerScreenBehaviour.SetRandomAvatar();
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
    }
}