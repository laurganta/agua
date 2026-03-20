using System;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class MainMenuScreenBehaviour : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        // [SerializeField] private Button _nextLeaderboardButton;
        // [SerializeField] private Button _previousLeaderboardButton;

        [SerializeField] private GameObject _devMenu;
        [SerializeField] private Button _showDevMenu;
        [SerializeField] private Button _hideDevMenu;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _startNewMatchButton;

        [SerializeField] private LeaderboardBehaviour _leaderboardBehaviour;
        
        public Action OnPlay;
        // public Action OnNextLeaderboard;
        // public Action OnPreviousLeaderboard;
        public Action OnReset;
        public Action OnStartNewMatch;
        public Action<string> OnSelectPlayer;

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayButtonClick);
            // _nextLeaderboardButton.onClick.AddListener(NextLeaderboard);
            // _previousLeaderboardButton.onClick.AddListener(PreviousLeaderboard);
            _startNewMatchButton.onClick.AddListener(OnStartNewMatchClick);
            
            _devMenu.SetActive(false);
            _showDevMenu.onClick.AddListener(ShowDevMenu);
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
        
        public void ShowDevMenu()
        {
            _devMenu.SetActive(true);
        }

        public void HideDevMenu()
        {
            _devMenu.SetActive(false);
        }

        // private void PreviousLeaderboard()
        // {
        //     OnPreviousLeaderboard?.Invoke();
        // }
        //
        // private void NextLeaderboard()
        // {
        //     OnNextLeaderboard?.Invoke();
        // }

        private void OnPlayButtonClick()
        {
            OnPlay?.Invoke();
        }

        public void SelectPlayerInDebugMenu(string playerName)
        {
            _devMenu.SetActive(true);
            _devMenu.GetComponentInChildren<PlayerInfoListBehaviour>()
                .SearchPlayer(playerName);
        }
    }
}