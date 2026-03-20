using System;
using UnityEngine;

namespace CleverEdge
{
    public class GameplayStateBehaviour : GameStateBehaviourBase
    {
        private enum State
        {
            Init,
            WaitingToStart,
            Playing,
            Boss,
            WaitingToEnd,
            End
        }

        private struct SessionData
        {
            public float Score;
        }

        [SerializeField] private EnemiesControllerBehaviour _enemiesControllerBehaviour;
        [SerializeField] private GameplayScreenBehaviour _gameplayScreenBehaviour;
        [SerializeField] private CameraShakeBehaviour _cameraShakeBehaviour;
        [SerializeField] private PowerUpsControllerBehaviour _powerUpsControllerBehaviour;
        [SerializeField] private FlyTextControllerBehaviour _flyTextController;
        [SerializeField] private PlayerBehaviour _playerBehaviour;
        
        [SerializeField] private float _roundDuration;
        [SerializeField] private float _waitingToEndDuration;
        [SerializeField] private float _waitingToStartDuration;
        
        private InputSystem_Actions _inputSystemActions;
        
        private State _state;
        private float _roundTimer;
        private float _startTimer;
        private float _currentRoundDuration;

        private bool _bossDefeated;

        private SessionData _sessionData;
        public Action OnGameEnd;

        public void SetRoundDuration(float duration)
        {
            _roundDuration = duration;
        }
        
        private void Awake()
        {
            ServiceLocator.AddInstance(this);
            
            _inputSystemActions = ServiceLocator.GetInstance<InputSystem_Actions>();

            _enemiesControllerBehaviour.Initialize(OnEnemyDefeated);
        }

        private void OnEnable()
        {
            _gameplayScreenBehaviour.gameObject.SetActive(true);
            
            ChangeState(State.Init);
        }

        private void OnDisable()
        {
            if (_gameplayScreenBehaviour)
                _gameplayScreenBehaviour.gameObject.SetActive(false);
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Init:
                    _playerBehaviour.PrepareForRound();
                    _startTimer = 0;
                    _roundTimer = 0;
                    _currentRoundDuration = _roundDuration;
                    _bossDefeated = false;
                    
                    _sessionData = new SessionData();
                    _gameplayScreenBehaviour.SetScore(_sessionData.Score);
                    _gameplayScreenBehaviour.SetTimeLeft(_currentRoundDuration - _roundTimer);
                    
                    ChangeState(State.WaitingToStart);
                    
                    break;
                case State.WaitingToStart:
                    
                    // ToDo - gameplay intro 
                    _startTimer += Time.deltaTime;
                    
                    if (_startTimer >= _waitingToStartDuration)
                        ChangeState(State.Playing);
                    break;
                case State.Playing:
                    _roundTimer += Time.deltaTime;
                    var secondsLeft = Mathf.FloorToInt(_currentRoundDuration - _roundTimer);
                    if (secondsLeft != _gameplayScreenBehaviour.SecondsLeft)
                        _gameplayScreenBehaviour.SetTimeLeft(secondsLeft, true);
                    
                    if (_roundTimer >= _currentRoundDuration)
                        ChangeState(State.Boss);
                    break;
                case State.Boss:
                    if (_bossDefeated)
                        ChangeState(State.WaitingToEnd);
                    break;
                case State.WaitingToEnd:
                    _roundTimer += Time.deltaTime;
                    if (_roundTimer >= _waitingToEndDuration)
                         ChangeState(State.End);
                    break;
                case State.End:
                    break;
            }
        }

        private void ChangeState(State state)
        {
            _state = state;

            switch (_state)
            {
                case State.Init:
                    break;
                case State.WaitingToStart:
                    break;
                case State.Playing:
                    _inputSystemActions.Enable();
                    _enemiesControllerBehaviour.StartSpawning();
                    _powerUpsControllerBehaviour.StartSpawning();
                    break;
                case State.Boss:
                    _gameplayScreenBehaviour.SetTimeLeft(0);
                    _enemiesControllerBehaviour.Stop();
                    _powerUpsControllerBehaviour.Stop();
                    _enemiesControllerBehaviour.SpawnBoss();
                    break;
                case State.WaitingToEnd:
                    _roundTimer = 0;
                    _enemiesControllerBehaviour.ClearRemainingEnemies();
                    _inputSystemActions.Disable();
                    break;
                
                case State.End:
                    
                    GameDebug.Log("Win!");
                    
                    LeaderboardState.Provider.SetEntry(
                        new LeaderboardEntry(Player.Current, Mathf.FloorToInt(_sessionData.Score), DateTime.Now));

                    OnGameEnd?.Invoke();
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void OnEnemyDefeated(Enemy enemy, Vector3 position)
        {
            _sessionData.Score += enemy.score;
            _gameplayScreenBehaviour.SetScoreAnimated(_sessionData.Score);
            _cameraShakeBehaviour.Shake();

            var scoreText = "+" + ((int) enemy.score).ToString("0");
            _flyTextController.SpawnScoreFlyText(position, scoreText , enemy.scoreColor, enemy.scoreFlyTextSize);
            
            if (enemy.tier == EnemyTier.Boss)
                _bossDefeated = true;
        }

        public void AddExtraTime(int extraTimeSeconds)
        {
            if (_state != State.Playing)
                return;

            _currentRoundDuration += extraTimeSeconds;
            var secondsLeft = Mathf.FloorToInt(_currentRoundDuration - _roundTimer);
            _gameplayScreenBehaviour.SetTimeLeft(secondsLeft, true);
        }
    }
}