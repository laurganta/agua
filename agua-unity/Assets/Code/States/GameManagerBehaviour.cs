using System;
using System.Collections.Generic;
using UnityEngine;

namespace CleverEdge
{
    public class GameManagerBehaviour : MonoBehaviour
    {
        [Header("Build Settings")]
        [SerializeField] private float _roundDuration;
        [SerializeField] private bool _startWithoutRegistering;
        [SerializeField] private float _bossMovementDuration;
        [SerializeField] private EnemyBehaviour _bossPrefab;
        [SerializeField] private float _idleTime;
        
        [ContextMenu("PrepareBuild")]
        public void PrepareBuild()
        {
            FindAnyObjectByType<MainMenuScreenBehaviour>(FindObjectsInactive.Include).gameObject.SetActive(false);
            FindAnyObjectByType<RegisterScreenBehaviour>(FindObjectsInactive.Include).gameObject.SetActive(false);
            FindAnyObjectByType<GameplayScreenBehaviour>(FindObjectsInactive.Include).gameObject.SetActive(false);
            FindAnyObjectByType<GDPRScreenBehaviour>(FindObjectsInactive.Include).gameObject.SetActive(false);

            FindAnyObjectByType<GameplayStateBehaviour>(FindObjectsInactive.Include).SetRoundDuration(_roundDuration);
            FindAnyObjectByType<RegisterScreenBehaviour>(FindObjectsInactive.Include).SetStartWithoutRegistering(_startWithoutRegistering);

            _bossPrefab.GetComponent<EnemyMovementBehaviour>()
                .SetDurationRange(new Vector2(_bossMovementDuration, _bossMovementDuration));
            
            _mainMenuStateBehaviour.SetIdleTime(_idleTime);
        }

        private enum GameState
        {
            MainMenu,
            Gameplay,
            EndScreen,
            Register,
            GDPR,
        }
        
        
        [Header("References")]
        [SerializeField] private MainMenuStateBehaviour _mainMenuStateBehaviour;
        [SerializeField] private GameplayStateBehaviour _gameplayStateBehaviour;
        [SerializeField] private EndScreenStateBehaviour _endScreenStateBehaviour;
        [SerializeField] private RegisterStateBehaviour _registerStateBehaviour;
        [SerializeField] private GDPRStateBehaviour _gdprStateBehaviour;
        
        [SerializeField] private AvatarsConfig _avatarsConfig;

        [SerializeField] private bool _instantStart;
        [SerializeField] private int _matchDurationMinutes;

        private Dictionary<GameState, GameStateBehaviourBase> _states;

        private Player _currentPlayer;
        
        public int MatchDurationMinutes => _matchDurationMinutes;
        
        private void Awake()
        {
            _states = new Dictionary<GameState, GameStateBehaviourBase>
            {
                {GameState.MainMenu, _mainMenuStateBehaviour},
                {GameState.Gameplay, _gameplayStateBehaviour},
                {GameState.EndScreen, _endScreenStateBehaviour},
                {GameState.Register, _registerStateBehaviour},
                {GameState.GDPR, _gdprStateBehaviour},
            };
            
            _mainMenuStateBehaviour.OnPlay += OnPlay;
            _mainMenuStateBehaviour.OnRetry += OnRetry;
            
            _gameplayStateBehaviour.OnGameEnd += OnGameEnd;
            
            _endScreenStateBehaviour.OnReturnToMainMenu += ToMainMenu;
            
            _registerStateBehaviour.OnRegister += OnRegister;
            _registerStateBehaviour.OnBackButton += ToMainMenu;
            _registerStateBehaviour.OnGDPRButton += ToGDPRScreen;
            
            _gdprStateBehaviour.OnAccept += OnGDPRAccept;
            _gdprStateBehaviour.OnBack += OnGDPRBack;

            Application.targetFrameRate = 144;
            
            ServiceLocator.AddInstance(_avatarsConfig);
            ServiceLocator.AddInstance(this);
            
            LeaderboardState.Initialize();
        }

        private void OnGameEnd(float score)
        {
            _registerStateBehaviour.SetCurrentScore(score);
            ToState(GameState.Register);
        }

        private void OnRetry()
        {
            ToState(GameState.Register);
            _registerStateBehaviour.SetPlayer(LeaderboardState.Provider.LastPlayer);
        }

        private void ToGDPRScreen()
        {
            ToState(GameState.GDPR);
        }

        private void OnGDPRBack()
        {
            ToState(GameState.Register);
        }

        private void OnGDPRAccept()
        {
            ToState(GameState.Register);
            _registerStateBehaviour.SetGDPRAccepted();
        }

        private void OnPlay()
        {
            if (LeaderboardState.Provider.HasMatchExpired(MatchDurationMinutes))
                LeaderboardState.Provider.CreateNewMatch();
            
            StartPlaying();
        }

        private void OnRegister()
        {
            ToMainMenu();
        }

        private void Start()
        {
            if (_instantStart)
                StartPlaying();
            else
                ToMainMenu();
        }

        private void ToState(GameState state)
        {
            foreach (var kvp in _states)
                kvp.Value.gameObject.SetActive(kvp.Key == state);
        }

        private void StartPlaying()
        {
            ToState(GameState.Gameplay);
        }

        private void EndGame()
        {
            ToState(GameState.EndScreen);
        }

        private void ToMainMenu()
        {
            ToState(GameState.MainMenu);
        }
    }
}