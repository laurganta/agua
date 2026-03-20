using System.Collections.Generic;
using UnityEngine;

namespace CleverEdge
{
    [CreateAssetMenu(fileName = "AvatarsConfig", menuName = "CleverEdge/AvatarsConfig")]
    public class AvatarsConfig : ScriptableObject
    {
        [SerializeField] private List<GameObject> _avatarPrefabs;
        
        public int AvatarsCount => _avatarPrefabs.Count;
        
        public int GetRandomAvatarIndex() => Random.Range(0, _avatarPrefabs.Count);

        public GameObject GetAvatarPrefab(int index)
        {
            return _avatarPrefabs[index];
        }
    }
}