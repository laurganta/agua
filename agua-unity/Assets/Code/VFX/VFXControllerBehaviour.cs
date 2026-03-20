using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace CleverEdge
{
    public enum VFXEffectType
    {
        EnemyExplosion_Default,
        EnemyExplosion_Elite,
        EnemyExplosion_Boss,
        EnemyHit_Default,
        EnemyHit_Elite,
        EnemyHit_Boss,
        BulletBubbles,
        BulletHit,
        PowerUpCollect,
        BombExplosion,
    }

    public static class VFXExtensions
    {
        public static VFXEffectType ToExplosionEffectType(this EnemyTier tier)
        {
            return tier switch
            {
                EnemyTier.Default => VFXEffectType.EnemyExplosion_Default,
                EnemyTier.Elite => VFXEffectType.EnemyExplosion_Elite,
                EnemyTier.Boss => VFXEffectType.EnemyExplosion_Boss,
                _ => VFXEffectType.EnemyExplosion_Default
            };
        }
        
        public static VFXEffectType ToHitEffectType(this EnemyTier tier)
        {
            return tier switch
            {
                EnemyTier.Default => VFXEffectType.EnemyHit_Default,
                EnemyTier.Elite => VFXEffectType.EnemyHit_Elite,
                EnemyTier.Boss => VFXEffectType.EnemyHit_Boss,
                _ => VFXEffectType.EnemyHit_Default
            };
        }
    }

    public class VFXControllerBehaviour : MonoBehaviour
    {

        [System.Serializable]
        public struct EffectInfo
        {
            public VFXEffectType effectType;
            public VFXEffectBehaviour prefab;
            public int startPoolCapacity;
            public int maxPoolCapacity;
        }

        [SerializeField] private List<EffectInfo> _effects;

        private Dictionary<VFXEffectType, ObjectPool<VFXEffectBehaviour>> _pools;

        private void Awake()
        {
            ServiceLocator.AddInstance(this);
            
            InitializePools();
        }

        private void InitializePools()
        {
            // initialize a pool for each time of effect 
            _pools = new Dictionary<VFXEffectType, ObjectPool<VFXEffectBehaviour>>();
            foreach (var effectInfo in _effects)
            {
                var pool = new ObjectPool<VFXEffectBehaviour>(
                    createFunc: () =>
                    {
                        var effect = Instantiate(effectInfo.prefab, transform, true);

                        effect.Initialize(OnEffectDispose, effectInfo.effectType);
                        
                        return effect;
                    },
                    actionOnGet: (effect) => effect.gameObject.SetActive(true),
                    actionOnRelease: (effect) => effect.gameObject.SetActive(false),
                    actionOnDestroy: (effect) =>
                    {
                        if (effect)
                            Destroy(effect.gameObject);
                    },
                    collectionCheck: false,
                    defaultCapacity: effectInfo.startPoolCapacity,
                    maxSize: effectInfo.maxPoolCapacity);
                    
                _pools.Add(effectInfo.effectType, pool);
            }
        }

        private void OnEffectDispose(VFXEffectBehaviour obj)
        {
            if (_pools.TryGetValue(obj.EffectType, out var pool))
            {
                pool.Release(obj);
            }
            else
            {
                GameDebug.LogWarning($"No pool found for effect type {obj.EffectType}");
                Destroy(obj.gameObject);
            }
        }

        public void PlayEffect(VFXEffectType type, Vector3 position, Quaternion rotation, Transform targetToFollow = null)
        {
            if (_pools.TryGetValue(type, out var pool))
            {
                var effect = pool.Get();
                effect.transform.position = position;
                effect.transform.rotation = rotation;
                effect.Play(targetToFollow);
            }
            else
            {
                GameDebug.LogWarning($"No pool found for effect type {type}");
            }
        }
    }
}