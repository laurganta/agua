using System;
using UnityEngine;

namespace CleverEdge
{
    public class GameplayStateBehaviour : GameStateBehaviourBase
    {
        private enum State
        {
            Init,
            Tutorial,
            Intro,
            Playing,
            Boss,
            WaitingToEnd,
            End
        }

        private struct SessionData
        {
            public float Score;
        }

        [Header("References")]
        [SerializeField] private EnemiesControllerBehaviour _enemiesControllerBehaviour;
        [SerializeField] private GameplayScreenBehaviour _gameplayScreenBehaviour;
        [SerializeField] private PowerUpsControllerBehaviour _powerUpsControllerBehaviour;
        [SerializeField] private FlyTextControllerBehaviour _flyTextController;
        [SerializeField] private PlayerBehaviour _playerBehaviour;
        [SerializeField] private ParticleSystem _confettiParticleSystem;

        [Header("Tutorial")] 
        [SerializeField] private float _tutorialDummyEnemiesDelay;
        [SerializeField] private float _tutorialFingerDelay;
        
        [Header("Round Settings")]
        [SerializeField] private float _roundDurationSeconds;
        [SerializeField] private float _waitingToEndDuration;

        [Header("Misc")] // This Should Not Be Here
        [SerializeField] private Color _scoreMultiplierFlyTextColor;
        [SerializeField] private float _scoreMultiplierFlyTextSize;
        
        private InputSystem_Actions _inputSystemActions;
        
        private State _state;
        private float _roundTimer;
        private float _currentStateTimer;
        private float _currentRoundDuration;
        private float _currentScoreMultiplier;
        private bool _hasSeenDummyTutorialEnemies;

        private bool _hasSeenTutorialFinger;

        private bool _bossDefeated;

        private SessionData _sessionData;
        public Action<float> OnGameEnd;
        
        private bool HasScoreMultiplier => _currentScoreMultiplier > 1;

        public void SetRoundDuration(float duration)
        {
            _roundDurationSeconds = duration;
        }
        
        private void Awake()
        {
            ServiceLocator.AddInstance(this);
            
            _inputSystemActions = ServiceLocator.GetInstance<InputSystem_Actions>();

            _enemiesControllerBehaviour.Initialize(OnEnemyDefeated, OnEnemyDamaged);
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
                    _gameplayScreenBehaviour.PrepareForRound();
                    
                    _roundTimer = 0;
                    _currentRoundDuration = _roundDurationSeconds;
                    _bossDefeated = false;
                    _currentScoreMultiplier = 1;
                    _currentStateTimer = 0;
                    _hasSeenDummyTutorialEnemies = false;
                    _hasSeenTutorialFinger = false;
                    
                    _sessionData = new SessionData();
                    _gameplayScreenBehaviour.SetScore(_sessionData.Score);
                    _gameplayScreenBehaviour.SetTimeLeft(_currentRoundDuration - _roundTimer);
                    
                    ChangeState(State.Tutorial);
                    
                    break;
                
                case State.Tutorial:
                    _inputSystemActions.Enable();

                    if (_hasSeenDummyTutorialEnemies == false)
                    {
                        _currentStateTimer += Time.deltaTime;
                        if (_currentStateTimer > _tutorialDummyEnemiesDelay)
                        {
                            _enemiesControllerBehaviour.SpawnTutorialEnemies();
                            _hasSeenDummyTutorialEnemies = true;
                        }
                    } else if (_enemiesControllerBehaviour.ActiveEnemiesCount == 0)
                        ChangeState(State.Intro);
                    else if (_hasSeenTutorialFinger == false)
                    {
                        _currentStateTimer += Time.deltaTime;
                        if (_currentStateTimer >= _tutorialFingerDelay)
                        {
                            _gameplayScreenBehaviour.PlayTutorialFinger();
                            _hasSeenTutorialFinger = true;
                        }
                    }

                    break;
                
                case State.Intro:
                    if (_gameplayScreenBehaviour.IsPlayingInto == false)
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
                    {
                        ChangeState(State.WaitingToEnd);
                    }
                    else if (_enemiesControllerBehaviour.Boss.FinishedMoving)
                    {
                        _enemiesControllerBehaviour.Boss.StopMoving();
                        _enemiesControllerBehaviour.Boss.PlayBossAttackAnimation();
                        ChangeState(State.WaitingToEnd);
                    }
                    
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

            GameDebug.Log($"GameplayState: {state}.");
            switch (_state)
            {
                case State.Init:
                    break;
                
                case State.Tutorial:
                    
                    break;
                
                case State.Intro:
                    _gameplayScreenBehaviour.PlayIntro();
                    break;
                
                case State.Playing:
                    _gameplayScreenBehaviour.StartPlaying();
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

                    if (_bossDefeated)
                    {
                        _confettiParticleSystem.Play(true);
                        _gameplayScreenBehaviour.ShowWinText();
                    }
                    else
                    {
                        _gameplayScreenBehaviour.ShowLoseText();
                    }

                    _roundTimer = 0;
                    _inputSystemActions.Disable();
                    break;
                
                case State.End:
                    
                    _enemiesControllerBehaviour.ClearRemainingEnemies();
                    
                    OnGameEnd.Invoke(_sessionData.Score);
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetScoreMultiplier(float scoreMultiplier)
        {
            _currentScoreMultiplier = scoreMultiplier;
            _gameplayScreenBehaviour.SetScoreMultiplier(scoreMultiplier);
        }

        private void OnEnemyDamaged(EnemyBehaviour obj)
        {
            _gameplayScreenBehaviour.PingActivityTimer();
        }
        
        private void OnEnemyDefeated(EnemyBehaviour enemyBehaviour, Enemy enemyConfig)
        {
            var score = enemyConfig.score * _currentScoreMultiplier;

            if (enemyConfig.tier == EnemyTier.Boss)
            {
                _bossDefeated = true;

                score *= 1 - enemyBehaviour.PathCompletedPercentage;
            }

            _sessionData.Score += score;
            _gameplayScreenBehaviour.SetScoreAnimated(_sessionData.Score);

            var scoreText = "+" + ((int) score).ToString("0");

            var flyTextColor = HasScoreMultiplier ? _scoreMultiplierFlyTextColor : enemyConfig.scoreColor;
            var flyTextSize = HasScoreMultiplier ? enemyConfig.scoreFlyTextSize * _scoreMultiplierFlyTextSize : enemyConfig.scoreFlyTextSize;
            
            _flyTextController.SpawnFlyText(enemyBehaviour.transform.position, scoreText , flyTextColor, flyTextSize);
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