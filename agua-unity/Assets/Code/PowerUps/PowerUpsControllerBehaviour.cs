using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CleverEdge
{
    public class PowerUpsControllerBehaviour : MonoBehaviour
    {
        [SerializeField] private PowerUpBehaviour[] _powerUpsPrefabs;
        [SerializeField] private Transform _powerUpsParent;
        [SerializeField] private float[] _spawnTimes;
        [SerializeField] private Transform _spawnAreaCenter;
        [SerializeField] private Vector2 _spawnAreaSize;
        [SerializeField] private FlyTextControllerBehaviour _flyTextController;

        [Header("Configuration - Extra Time")]
        [SerializeField] public int _extraTimeSeconds;
        [SerializeField] private Color _extraTimeFlyTextColor;
        [SerializeField] private float _extraTimeFlyTextSize;
        

        private Dictionary<PowerUpType, PowerUpBehaviour> _powerUps;

        private bool _spawning;
        private float _spawnTimer;
        private int _currentSpawnIndex;
        
        private Dictionary<PowerUpType, int> _spawnedPowerUps;
        private List<PowerUpType> _availablePowerUpTypesBuffer;

        private GameplayStateBehaviour _gameplayState;
        
        private void Awake()
        {
            _powerUps = new Dictionary<PowerUpType, PowerUpBehaviour>();
            _spawnedPowerUps = new Dictionary<PowerUpType, int>();
            _availablePowerUpTypesBuffer = new List<PowerUpType>();
            
            foreach (var powerUpBehaviour in _powerUpsPrefabs)
            {
                var powerUp = Instantiate(powerUpBehaviour, _powerUpsParent);
                
                powerUp.gameObject.SetActive(false);
                powerUp.Initialize(OnCollect,  OnPowerUpActiveDurationEnd);
                
                _powerUps.Add(powerUp.PowerUpType, powerUp);
            }
        }

        // Very bad architecture, but I'm rushing as the game is already out to players hands 
        public void Initialize(GameplayStateBehaviour gameplayState)
        {
            _gameplayState = gameplayState;
        }
        
        private void OnCollect(PowerUpBehaviour powerUp)
        {
            ReleasePowerUp(powerUp);

            switch (powerUp.PowerUpType)
            {
                case PowerUpType.ExtraTime:
                    _gameplayState.AddExtraTime(_extraTimeSeconds);
                    _flyTextController.SpawnScoreFlyText(powerUp.transform.position, $"+{_extraTimeSeconds}s", _extraTimeFlyTextColor, _extraTimeFlyTextSize);
                    break;
            }
        }

        private void OnPowerUpActiveDurationEnd(PowerUpBehaviour powerUp)
        {
            ReleasePowerUp(powerUp);
        }

        private void SpawnPowerUp(PowerUpType powerUpType, Vector3 position)
        {
            if (_powerUps.TryGetValue(powerUpType, out var powerUp))
            {
                powerUp.transform.position = position;
                powerUp.gameObject.SetActive(true);
                
                _spawnedPowerUps.TryAdd(powerUpType, 0);
                _spawnedPowerUps[powerUpType]++;
            }
        }

        public void ReleasePowerUp(PowerUpBehaviour powerUp)
        {
            powerUp.gameObject.SetActive(false);
        }

        public void StartSpawning()
        {
            _spawnTimer = 0;
            _currentSpawnIndex = 0;
            _spawning = true;
            _spawnedPowerUps.Clear();
        }

        public void Stop()
        {
            _spawning = false;
        }

        private void Update()
        {
            if (_spawning == false)
                return;
            
            _spawnTimer += Time.deltaTime;
            
            if (_currentSpawnIndex < _spawnTimes.Length && _spawnTimer >= _spawnTimes[_currentSpawnIndex])
            {
                var randomPosition = _spawnAreaCenter.position + new Vector3(
                    Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
                    0,
                    Random.Range(-_spawnAreaSize.y / 2, _spawnAreaSize.y / 2));
                
                _availablePowerUpTypesBuffer.Clear();

                foreach (var (powerUpType, powerUp) in _powerUps)
                    if (powerUp.IsActive && (_spawnedPowerUps.ContainsKey(powerUpType) == false || _spawnedPowerUps[powerUpType] < powerUp.LimitPerRound))
                        _availablePowerUpTypesBuffer.Add(powerUpType);
                
                if (_availablePowerUpTypesBuffer.Count == 0)
                {
                    GameDebug.LogWarning("No available power up types to spawn.");
                    return;
                }
                
                var randomPowerUpType = _availablePowerUpTypesBuffer[Random.Range(0, _availablePowerUpTypesBuffer.Count)];
                
                SpawnPowerUp(randomPowerUpType, randomPosition);
                
                _currentSpawnIndex++;
            }
        }
    }
}