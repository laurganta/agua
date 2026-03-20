using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class RegisterScreenBehaviour : MonoBehaviour
    {
        [SerializeField] private bool _playWithoutRegistering;
        
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _randomAvatarButton;
        [SerializeField] private Button _backbutton;
        [SerializeField] private Button _gdprButton;
        // [SerializeField] private Button _selectPlayerButton;
        
        [SerializeField] private AvatarBehaviour _avatarBehaviour;
        [SerializeField] private TMP_InputField _nameField;
        [SerializeField] private TMP_InputField _phoneField;
        [SerializeField] private TMP_InputField _emailField;
        [SerializeField] private Toggle _gdprToggle;

        [SerializeField] private float _randomAvatarPunchDuration;
        [SerializeField] private float _randomAvatarPunchScale;
        
        public Action OnRegister;
        public Action OnBackButton;
        public Action OnGDPRButton;

        private Tweener _randomAvatarPunchTweener;

        public string PlayerName => _nameField.text;
        public string Phone => _phoneField.text;
        public string Email => _emailField.text;
        
        public int AvatarIndex => _avatarBehaviour.AvatarIndex;
        public bool GdprAccepted => _gdprToggle.isOn;
        
        public void SetStartWithoutRegistering(bool value)
        {
            _playWithoutRegistering = value;
        }
        
        private void Awake()
        {
            _registerButton.onClick.AddListener(OnRegisterButtonClick);
            _randomAvatarButton.onClick.AddListener(OnAvatarClick);
            _backbutton.onClick.AddListener(OnBackButtonClick);
            _gdprButton.onClick.AddListener(OnGDPRButtonClick);
            // _selectPlayerButton.onClick.AddListener(SelectPlayerButtonClick);

            _registerButton.interactable = false;
            _gdprToggle.isOn = false;
        }

        private void SelectPlayerButtonClick()
        {
            
        }

        private void Update()
        {
            _registerButton.interactable = _playWithoutRegistering || (string.IsNullOrEmpty(PlayerName) == false 
                                            && string.IsNullOrEmpty(Phone) == false 
                                            && string.IsNullOrEmpty(Email) == false
                                            && GdprAccepted);
        }

        private void OnAvatarClick()
        {
            SetRandomAvatar();

            if (_randomAvatarPunchTweener != null)
                _randomAvatarPunchTweener.Kill(true);
            
            _randomAvatarPunchTweener = _avatarBehaviour.transform.parent.DOPunchScale(_randomAvatarPunchScale * Vector3.one, _randomAvatarPunchDuration, 1);
        }

        public void SetRandomAvatar()
        {
            var randomIndex = _avatarBehaviour.AvatarIndex;
            
            do
            {
                randomIndex = GetRandomAvatarIndex();
            } while (randomIndex == _avatarBehaviour.AvatarIndex);
            
            _avatarBehaviour.SetAvatar(randomIndex);
        }

        private void OnGDPRButtonClick()
        {
            OnGDPRButton?.Invoke();
        }

        private void OnRegisterButtonClick()
        {
            OnRegister?.Invoke();
        }
        
        private void OnBackButtonClick()
        {
            OnBackButton?.Invoke();
        }

        public void SetPlayer(Player player)
        {
            _nameField.text = player.PlayerName;
            _phoneField.text = player.PhoneNumber;
            _emailField.text = player.Email;
            _avatarBehaviour.SetAvatar(player.AvatarIndex);
            _gdprToggle.isOn = true;
        }

        private int GetRandomAvatarIndex()
        {
            var avatarsConfig = ServiceLocator.GetInstance<AvatarsConfig>();
            return avatarsConfig.GetRandomAvatarIndex();
        }

        public void SetGDPRAccepted()
        {
            _gdprToggle.isOn = true;
        }
    }
}