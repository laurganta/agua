using System;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class MainMenuScreenBehaviour : MonoBehaviour
    {
        [SerializeField] private Button _playButton;

        [SerializeField] private GameObject _devMenu;
        [SerializeField] private Button _showDevMenu;
        [SerializeField] private Button _hideDevMenu;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _startNewMatchButton;

        [SerializeField] private LeaderboardBehaviour _leaderboardBehaviour;
        
        public Action OnPlay;
        public Action OnReset;
        public Action OnStartNewMatch;
        public Action<string> OnSelectPlayer;

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayButtonClick);
            _startNewMatchButton.onClick.AddListener(OnStartNewMatchClick);
            
            _devMenu.SetActive(false);
            _showDevMenu.onClick.AddListener(() => ShowDevMenu());
            _hideDevMenu.onClick.AddListener(HideDevMenu);
            _resetButton.onClick.AddListener(OnResetButtonClick);
            
            _leaderboardBehaviour.OnSelectPlayer += OnSelectPlayerClicked;
            _leaderboardBehaviour.Setup();
        }

        private void OnStartNewMatchClick()
        {
            OnStartNewMatch?.Invoke();
        }

        private void OnResetButtonClick()
        {
            OnReset?.Invoke();
        }

        private void OnSelectPlayerClicked(string playerName)
        {
            OnSelectPlayer?.Invoke(playerName);
        }

        public void Initialize()
        {
            _leaderboardBehaviour.Initialize();
        }
        
        public bool ShowDevMenu()
        {
            ServiceLocator.GetInstance<PinPopupBehaviour>().Show(pin =>
            {
                GameDebug.Log("PIN entered: " + pin);
                if (pin == Constants.PIN)
                {
                    _devMenu.SetActive(true);
                }
                else
                {
                    GameDebug.Log("Incorrect PIN");
                }
            }, () => { });

            return false;
        }

        public void HideDevMenu()
        {
            _devMenu.SetActive(false);
        }

        private void OnPlayButtonClick()
        {
            OnPlay?.Invoke();
        }

        public void SelectPlayerInDebugMenu(string playerName)
        {
            var success = ShowDevMenu();
            if (success)
                _devMenu.GetComponentInChildren<PlayerInfoListBehaviour>().SearchPlayer(playerName);
        }
    }
}