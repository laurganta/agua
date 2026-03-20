using System;
using UnityEngine;

namespace CleverEdge
{
    public class MainMenuStateBehaviour : GameStateBehaviourBase
    {
        [SerializeField] private MainMenuScreenBehaviour _mainMenuScreenBehaviour;
        
        public Action OnPlay;

        private bool _shouldReset;

        private SwipeDetectionController _swipeDetectionController;
        
        private void Awake()
        {
            _mainMenuScreenBehaviour.OnPlay += Play;
            _mainMenuScreenBehaviour.OnSelectPlayer += OnSelectPlayer;
            _mainMenuScreenBehaviour.OnReset += OnReset;
            _mainMenuScreenBehaviour.OnStartNewMatch += OnStartNewMatch;
            
            _swipeDetectionController = new SwipeDetectionController();
            _swipeDetectionController.OnSwipe += OnSwipe;
            
            LeaderboardState.Provider.LoadLastLeaderboard();
        }

        private void OnStartNewMatch()
        {
            LeaderboardState.Provider.CreateNewMatch();
        }

        private void OnReset()
        {
            AndroidDialog.Show("Reset all data?", "This will backup all players and their scores. Are you sure?", "Reset", "No", () =>
            {
                _shouldReset = true;
            }, () => { });
        }

        private void OnSwipe(SwipeDirection direction)
        {
            if (direction == SwipeDirection.Left)
                OnNextLeaderboard();
            else if (direction == SwipeDirection.Right)
                OnPreviousLeaderboard();
        }

        private void OnSelectPlayer(string playerName)
        {
            _mainMenuScreenBehaviour.SelectPlayerInDebugMenu(playerName);
        }

        private void OnPreviousLeaderboard()
        {
            LeaderboardState.Provider.SetPreviousLeaderboard();
        }

        private void OnNextLeaderboard()
        {
            LeaderboardState.Provider.SetNextLeaderboard();
        }

        private void Play()
        {
            OnPlay?.Invoke();
        }

        private void OnEnable()
        {
            _mainMenuScreenBehaviour.gameObject.SetActive(true);
            _mainMenuScreenBehaviour.Initialize();
        }

        private void OnDisable()
        {
            if (_mainMenuScreenBehaviour)
                _mainMenuScreenBehaviour.gameObject.SetActive(false);
        }

        private void Update()
        {
            _swipeDetectionController.Update();

            if (_shouldReset)
            {
                _shouldReset = false;
                
                LeaderboardState.Provider.ClearAll();
                _mainMenuScreenBehaviour.HideDevMenu();
            }
        }
    }
}