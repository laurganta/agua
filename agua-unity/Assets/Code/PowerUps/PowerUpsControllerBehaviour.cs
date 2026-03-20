using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CleverEdge
{
    public class PowerUpsControllerBehaviour : MonoBehaviour
    {
        private class RuntimePowerUpData
        {
            public int SpawnedCount { get; private set; }
            public int CollectionCount { get; private set; }
            
            public void IncrementSpawned()
            {
                SpawnedCount++;
            }
            
            public void IncrementCollected()
            {
                CollectionCount++;
            }
        }

        [SerializeField] private PowerUpBehaviour[] _powerUpsPrefabs;
        [SerializeField] private Transform _powerUpsParent;
        [SerializeField] private float _firstSpawnDelay;
        [SerializeField] private Vector2 _spawnIntervalRange;
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
        private float _currentSpawnTime;
        
        private Dictionary<PowerUpType, RuntimePowerUpData> _runtimePowerUps;
        
        private List<PowerUpType> _availablePowerUpTypesBuffer;

        private GameplayStateBehaviour _gameplayState;
        private PlayerBehaviour _playerBehaviour;

        private void SelectNewSpawnTime()
        {
            _currentSpawnTime = Random.Range(_spawnIntervalRange.x, _spawnIntervalRange.y);
        }

        private void Awake()
        {
            _powerUps = new Dictionary<PowerUpType, PowerUpBehaviour>();
            _runtimePowerUps = new Dictionary<PowerUpType, RuntimePowerUpData>();
            _availablePowerUpTypesBuffer = new List<PowerUpType>();
            
            foreach (var powerUpBehaviour in _powerUpsPrefabs)
            {
                var powerUp = Instantiate(powerUpBehaviour, _powerUpsParent);
                
                powerUp.gameObject.SetActive(false);
                powerUp.Initialize(OnCollect,  OnPowerUpActiveDurationEnd);
                
                _powerUps.Add(powerUp.PowerUpType, powerUp);
            }
        }

        private void Start()
        {
            _gameplayState = ServiceLocator.GetInstance<GameplayStateBehaviour>();
            _playerBehaviour = ServiceLocator.GetInstance<PlayerBehaviour>();
        }

        private void OnCollect(PowerUpBehaviour powerUp)
        {
            _runtimePowerUps[powerUp.PowerUpType].IncrementCollected();
            
            ReleasePowerUp(powerUp);

            switch (powerUp.PowerUpType)
            {
                case PowerUpType.ExtraTime:
                    _gameplayState.AddExtraTime(_extraTimeSeconds);
                    _flyTextController.SpawnScoreFlyText(powerUp.transform.position, $"+{_extraTimeSeconds}s", _extraTimeFlyTextColor, _extraTimeFlyTextSize);
                    break;
                
                case PowerUpType.DoubleBarrel:
                    _playerBehaviour.SetLeftHandActive(true);
                    break;
            }
        }

        private void OnPowerUpActiveDurationEnd(PowerUpBehaviour powerUp)
        {
            ReleasePowerUp(powerUp);
        }

        public void ReleasePowerUp(PowerUpBehaviour powerUp)
        {
            powerUp.gameObject.SetActive(false);
        }

        public void StartSpawning()
        {
            _spawnTimer = 0;
            _spawning = true;

            _runtimePowerUps.Clear();
            _currentSpawnTime = _firstSpawnDelay;
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

            if (_spawnTimer >= _currentSpawnTime)
            {
                SpawnRandomPowerUp();
                
                SelectNewSpawnTime();
                _spawnTimer = 0;
            }
        }

        private void SpawnRandomPowerUp()
        {
            var randomPosition = _spawnAreaCenter.position + new Vector3(
                Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
                0,
                Random.Range(-_spawnAreaSize.y / 2, _spawnAreaSize.y / 2));
                
            _availablePowerUpTypesBuffer.Clear();

            foreach (var (powerUpType, powerUp) in _powerUps)
                if (powerUp.IsActive // if powerUp is active and its limits are not reached (spawn and collection) 
                        && (_runtimePowerUps.ContainsKey(powerUpType) == false 
                                || (_runtimePowerUps[powerUpType].SpawnedCount < powerUp.SpawnLimit && _runtimePowerUps[powerUpType].CollectionCount < powerUp.CollectionLimit)
                            )
                    )
                    _availablePowerUpTypesBuffer.Add(powerUpType);
                
            if (_availablePowerUpTypesBuffer.Count == 0)
            {
                GameDebug.LogWarning("No available power up types to spawn.");
                return;
            }
                
            var randomPowerUpType = _availablePowerUpTypesBuffer[Random.Range(0, _availablePowerUpTypesBuffer.Count)];
                
            SpawnPowerUp(_powerUps[randomPowerUpType], randomPosition);
        }
        
        private void SpawnPowerUp(PowerUpBehaviour powerUp, Vector3 position)
        {
            powerUp.transform.position = position;
            powerUp.gameObject.SetActive(true);
            
            _runtimePowerUps.TryAdd(powerUp.PowerUpType, new RuntimePowerUpData());
            _runtimePowerUps[powerUp.PowerUpType].IncrementSpawned();
        }

    }
}