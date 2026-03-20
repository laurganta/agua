using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private CameraShakeBehaviour _cameraShakeBehaviour;

        [Header("Configuration - Extra Time")]
        [SerializeField] public int _extraTimeSeconds;
        [SerializeField] private Color _extraTimeFlyTextColor;
        [SerializeField] private float _extraTimeFlyTextSize;
        
        [Header("Configuration - Bomb")]
        [SerializeField] private float _bombDelaySeconds;
        [SerializeField] private float _bombPositionShakeStrength;
        [SerializeField] private float _bombScaleShakeStrength;
        [SerializeField] private float _bombCameraShakeDuration;

        [Header("Configuration - Score Multiplier")] 
        [SerializeField] private float _scoreMultiplier;

        private Dictionary<PowerUpType, PowerUpBehaviour> _powerUps;

        private bool _spawning;
        private float _spawnTimer;
        private float _currentSpawnTime;
        
        private Dictionary<PowerUpType, RuntimePowerUpData> _runtimePowerUps;
        
        private List<PowerUpType> _availablePowerUpTypesBuffer;

        private GameplayStateBehaviour _gameplayState;
        private PlayerBehaviour _playerBehaviour;
        private VFXControllerBehaviour _vfxController;
        
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
            _vfxController = ServiceLocator.GetInstance<VFXControllerBehaviour>();
        }

        private void OnCollect(PowerUpBehaviour powerUp)
        {
            _runtimePowerUps[powerUp.PowerUpType].IncrementCollected();
            
            switch (powerUp.PowerUpType)
            {
                case PowerUpType.ExtraTime:
                    _gameplayState.AddExtraTime(_extraTimeSeconds);
                    _flyTextController.SpawnScoreFlyText(powerUp.transform.position, $"+{_extraTimeSeconds}s", _extraTimeFlyTextColor, _extraTimeFlyTextSize);
                    
                    ReleasePowerUp(powerUp);
                    break;
                
                case PowerUpType.DoubleBarrel:
                    _playerBehaviour.SetLeftHandActive(true);
                    
                    ReleasePowerUp(powerUp);
                    break;
                
                case PowerUpType.Bomb:
                    StartCoroutine(ExplodeBombAfterSeconds(powerUp, _bombDelaySeconds));
                    
                    break;
                
                case PowerUpType.ScoreMultiplier:
                    _gameplayState.SetScoreMultiplier(_scoreMultiplier);
                    
                    ReleasePowerUp(powerUp);
                    break;
                
                case PowerUpType.ExtraBullet:
                    _playerBehaviour.IncreaseMaxBulletIndex();
                    
                    ReleasePowerUp(powerUp);
                    break;
            }
        }        
        
        private IEnumerator ExplodeBombAfterSeconds(PowerUpBehaviour bomb, float bombDelaySeconds)
        {
            var timer = 0.0f;

            _vfxController.PlayEffect(VFXEffectType.BombExplosion, bomb.ScaleRoot.position, Quaternion.identity);
            var initialPosition = bomb.transform.position;

            while (timer < bombDelaySeconds)
            {
                timer += Time.deltaTime;
            
                var shakeOffset = Random.insideUnitSphere * _bombPositionShakeStrength;
                bomb.transform.position = initialPosition + shakeOffset * (1 - timer / bombDelaySeconds);
                
                var scaleOffset = Vector3.one * Mathf.Lerp(-_bombScaleShakeStrength, _bombScaleShakeStrength, Random.value);
                bomb.transform.localScale = Vector3.one + scaleOffset * (1.5f - timer / bombDelaySeconds);
                
                yield return null;
            }

            ReleasePowerUp(bomb);
            
            FindObjectsByType<EnemyBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .ToList()
                .ForEach(enemy => enemy.Damage(1000000, enemy.transform.position));
            
            _cameraShakeBehaviour.Shake(_bombCameraShakeDuration);
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