using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace CleverEdge
{
    public class EnemiesControllerBehaviour : MonoBehaviour
    {
        [SerializeField] private EnemyMovementPathControllerBehaviour _movementPathControllerBehaviour;
        [SerializeField] private VFXControllerBehaviour _vfxControllerBehaviour;
        [SerializeField] private List<Enemy> _enemies;
        [SerializeField] private Vector2 _spawnIntervalRange;
        [SerializeField] private Transform _enemiesSpawnCenter;
        [SerializeField] private Vector2 _spawnAreaSize;
        [SerializeField] private Transform[] _dummyEnemiesPosition;
        [SerializeField] private EnemyMovementPathBehaviour _bossPath;

        private Dictionary<EnemyTier, ObjectPool<EnemyBehaviour>> _enemiesPools;
        
        private List<EnemyBehaviour> _activeEnemies;
        
        private bool _running;

        private float _spawnTimer;
        private float _currentSpawnInterval;

        private Action<EnemyBehaviour, Enemy> _onEnemyDefeated;
        private Action<EnemyBehaviour> _onEnemyDamaged;

        private float _currentRoundTime;
        
        public int ActiveEnemiesCount => _activeEnemies.Count;
        public EnemyBehaviour Boss { get; private set; }

        private void Awake()
        {
            _enemiesPools = new Dictionary<EnemyTier, ObjectPool<EnemyBehaviour>>();
            _activeEnemies = new List<EnemyBehaviour>();
            
            foreach (var enemy in _enemies)
            {
                var pool = CreateEnemyPool(enemy.tier, enemy.prefab, enemy.startPoolCapacity, enemy.maxPoolCapacity);
                _enemiesPools.Add(enemy.tier, pool);
            }
        }

        public void Initialize(Action<EnemyBehaviour, Enemy> onEnemyDefeated, Action<EnemyBehaviour> onEnemyDamaged)
        {
            _onEnemyDefeated = onEnemyDefeated;
            _onEnemyDamaged = onEnemyDamaged;
        }

        private Enemy GetEnemyByTier(EnemyTier tier)
        {
            return _enemies.First(x => x.tier == tier);
        }
        
        private ObjectPool<EnemyBehaviour> CreateEnemyPool(EnemyTier tier, EnemyBehaviour prefab, int startCapacity, int maxCapacity)
        {
            return new ObjectPool<EnemyBehaviour>(
                createFunc: () =>
                {
                    var enemy = Instantiate(prefab.gameObject).GetComponent<EnemyBehaviour>();
                    enemy.transform.SetParent(transform);
                    enemy.gameObject.SetActive(false);
                    enemy.Initialize(tier, OnEnemyDefeated, OnDamaged);
                    return enemy;
                },
                actionOnGet: (enemy) =>
                {
                    enemy.gameObject.SetActive(true);
                    if (enemy.Tier == EnemyTier.Default || enemy.Tier == EnemyTier.Elite)
                    {
                        var path = _movementPathControllerBehaviour.GetRandomFreePath();
                        var startPosition = enemy.SetMovementPath(path, Random.value < 0.5f ? 1 : -1);
                        enemy.transform.position = startPosition;
                    }
                    else if (enemy.Tier == EnemyTier.Boss)
                    {
                        var startPosition = enemy.SetMovementPath(_bossPath, 1);
                        enemy.transform.position = startPosition;

                        Boss = enemy;
                    }
                    else if (enemy.Tier == EnemyTier.Rogue)
                    {
                        var randomPosition = _enemiesSpawnCenter.position + new Vector3(
                            Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
                            0,
                            Random.Range(-_spawnAreaSize.y / 2, _spawnAreaSize.y / 2)
                        );
                        enemy.transform.position = randomPosition;
                        enemy.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        enemy.HideAfterSeconds(2);
                    } else if (enemy.Tier == EnemyTier.Dummy)
                    {
                    }

                    enemy.Reset();
                    _activeEnemies.Add(enemy);
                },
                actionOnRelease: (enemy) =>
                {
                    enemy.gameObject.SetActive(false); 
                    _movementPathControllerBehaviour.FreePath(enemy.GetMovementPath());
                    _activeEnemies.Remove(enemy);

                    if (enemy.Tier == EnemyTier.Boss)
                        Boss = null;
                },
                actionOnDestroy: (enemy) =>
                {
                    if (enemy)
                    {
                        Destroy(enemy.gameObject);
                    }
                },
                collectionCheck: false,
                defaultCapacity: startCapacity,
                maxSize: maxCapacity
            );
        }

        private void OnDamaged(EnemyBehaviour enemy)
        {
            _onEnemyDamaged?.Invoke(enemy);
        }

        private void OnEnemyDefeated(EnemyBehaviour enemyBehaviour, bool wasKilledByPlayer)
        {
            _enemiesPools[enemyBehaviour.Tier].Release(enemyBehaviour);

            if (wasKilledByPlayer)
            {
                var enemy = GetEnemyByTier(enemyBehaviour.Tier);
                _onEnemyDefeated.Invoke(enemyBehaviour, enemy);
            }
        }

        private void SpawnRandomEnemy(float currentRoundTimeSeconds)
        {
            if (_movementPathControllerBehaviour.HasFreePaths() == false)
            {
                GameDebug.LogWarning($"Can't spawn enemy, no free paths");
                return;
            }
            
            var randomValue = Random.value * _enemies.Sum(x => x.spawnDelaySeconds > currentRoundTimeSeconds ? 0 : x.spawnChance);
            
            var cumulativeChance = 0f;

            for (var i = 0; i < _enemies.Count; i++)
            {
                cumulativeChance += _enemies[i].spawnDelaySeconds > currentRoundTimeSeconds ? 0 : _enemies[i].spawnChance;

                if (randomValue <= cumulativeChance)
                {
                    SpawnEnemy((EnemyTier)i);
                    break;
                }
            }
        }
        
        private EnemyBehaviour SpawnEnemy(EnemyTier tier)
        {
            return _enemiesPools[tier].Get();
        }
        
        private void Update()
        {
            if (_running == false)
                return;

            _spawnTimer += Time.deltaTime;
            _currentRoundTime += Time.deltaTime;
            
            if (_spawnTimer > _currentSpawnInterval)
            {
                SpawnRandomEnemy(_currentRoundTime);
                _spawnTimer = 0;
                _currentSpawnInterval = Random.Range(_spawnIntervalRange.x, _spawnIntervalRange.y);
            }
        }
        
        public void StartSpawning()
        {
            _running = true;
            _currentRoundTime = 0;
        }

        public void Stop()
        {
            _running = false;
        }

        public void SpawnBoss()
        {
            SpawnEnemy(EnemyTier.Boss);
        }

        public void ClearRemainingEnemies()
        {
            var activeEnemies = FindObjectsByType<EnemyBehaviour>(FindObjectsSortMode.None);

            foreach (var enemy in activeEnemies)
                _enemiesPools[enemy.Tier].Release(enemy);
        }
        
        public void SpawnTutorialEnemies()
        {
            foreach (var dummyPosition in _dummyEnemiesPosition)
            {
                var dummy = SpawnEnemy(EnemyTier.Dummy);
                dummy.transform.position = dummyPosition.position;
            }
        }
    }
}