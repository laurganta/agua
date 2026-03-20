using System;
using UnityEngine;

namespace CleverEdge
{
    [Serializable]
    public class Enemy
    {
        [Header("Definition")]
        public EnemyTier tier;
        public EnemyBehaviour prefab;
        
        [Header("Pool")]
        public int startPoolCapacity;
        public int maxPoolCapacity;
        
        [Header("Spawn")]
        public float spawnChance;
        public float spawnCooldownSeconds;
        public float spawnDelaySeconds;
        
        [Header("Score")]
        public float score;
        public Color scoreColor;
        public float scoreFlyTextSize;
    }
}