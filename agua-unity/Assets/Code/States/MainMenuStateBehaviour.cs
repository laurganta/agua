using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CleverEdge
{
    public class MainMenuStateBehaviour : GameStateBehaviourBase
    {
        [SerializeField] private MainMenuScreenBehaviour _mainMenuScreenBehaviour;
        [SerializeField] private float _idleDelaySeonds;
        
        [SerializeField] private GameObject[] _idleObjectsActive;
        [SerializeField] private GameObject[] _idleObjectsInactive;

        public Action OnPlay;
        public Action OnRetry;

        private bool _shouldReset;

        private SwipeDetectionController _swipeDetectionController;
        
        private bool _isIdle;
        private float _idleTimer;
        
        private void Awake()
        {
            _mainMenuScreenBehaviour.OnPlay += Play;
            _mainMenuScreenBehaviour.OnSelectPlayer += OnSelectPlayer;
            _mainMenuScreenBehaviour.OnReset += OnReset;
            _mainMenuScreenBehaviour.OnRetry += OnRetryButtonClick;
            _mainMenuScreenBehaviour.OnStartNewMatch += OnStartNewMatch;
            
            _swipeDetectionController = new SwipeDetectionController();
            _swipeDetectionController.OnSwipe += OnSwipe;
            
            LeaderboardState.Provider.LoadSelectedLeaderboard();
        }

        private void OnStartNewMatch()
        {
            _idleTimer = 0;
            LeaderboardState.Provider.CreateNewMatch();
            _mainMenuScreenBehaviour.HideDevMenu();
        }

        private void OnReset()
        {
            _idleTimer = 0;
            AndroidDialog.Show("Reset all data?", "This will backup all players and their scores. Are you sure?", "Reset", "No", () =>
            {
                _shouldReset = true;
            }, () => { });
        }


        private void OnRetryButtonClick()
        {
            _idleTimer = 0;
            OnRetry?.Invoke();
        }

        private void OnSwipe(SwipeDirection direction)
        {
            _idleTimer = 0;
            if (direction == SwipeDirection.Left)
                OnNextLeaderboard();
            else if (direction == SwipeDirection.Right)
                OnPreviousLeaderboard();
        }

        private void OnSelectPlayer(string playerName)
        {
            _idleTimer = 0;
            _mainMenuScreenBehaviour.SelectPlayerInDebugMenu(playerName);
        }

        private void OnPreviousLeaderboard()
        {
            _idleTimer = 0;
            LeaderboardState.Provider.SetPreviousLeaderboard();
        }

        private void OnNextLeaderboard()
        {
            _idleTimer = 0;
            LeaderboardState.Provider.SetNextLeaderboard();
        }

        private void Play()
        {
            _idleTimer = 0;
            OnPlay?.Invoke();
        }

        private void OnEnable()
        {
            _isIdle = false;
            LeaderboardState.Provider.LoadSelectedLeaderboard();
            _mainMenuScreenBehaviour.gameObject.SetActive(true);
            _mainMenuScreenBehaviour.Initialize();
         
            SetIdleMode(false);
            _mainMenuScreenBehaviour.SetRetryButtonVisible(LeaderboardState.Provider.LastPlayer != null);
        }

        private void OnDisable()
        {
            if (_mainMenuScreenBehaviour)
                _mainMenuScreenBehaviour.gameObject.SetActive(false);
        }

        private void SetIdleMode(bool enabled)
        {
            _isIdle = enabled;
            foreach (var idleObjects in _idleObjectsActive)
            {
                idleObjects.SetActive(enabled);
            }
            
            foreach (var idleObjects in _idleObjectsInactive)
            {
                idleObjects.SetActive(!enabled);
            }
        }

        private void Update()
        {
            if (_mainMenuScreenBehaviour.IsDevMenuActive())
                return;
            
            if (_isIdle)
            {
                if (Touchscreen.current.primaryTouch.press.isPressed)
                {
                    _idleTimer = 0;
                    SetIdleMode(false);
                    OnPlay.Invoke();
                }
                
                return;
            }
            
            _idleTimer += Time.deltaTime;
            
            if (_idleTimer > _idleDelaySeonds)
            {
                SetIdleMode(true);
                return;
            }
            
            _swipeDetectionController.Update();

            if (_shouldReset)
            {
                _shouldReset = false;
                
                LeaderboardState.Provider.ClearAll();
                _mainMenuScreenBehaviour.HideDevMenu();
            }
        }

        public void SetIdleTime(float idleTime)
        {
            _idleTimer = idleTime;
        }
    }
}