using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CleverEdge
{
    public class EnemyMovementPathControllerBehaviour : MonoBehaviour
    {
        [SerializeField] private List<EnemyMovementPathBehaviour> _paths;

        private List<EnemyMovementPathBehaviour> _freePaths;
        private List<EnemyMovementPathBehaviour> _usedPaths;

        private void Awake()
        {
            _freePaths = new List<EnemyMovementPathBehaviour>(_paths);
            _usedPaths = new List<EnemyMovementPathBehaviour>();
        }

        public EnemyMovementPathBehaviour GetRandomFreePath()
        {
            if (_freePaths.Count == 0)
                return null;

            var path = _freePaths[Random.Range(0, _freePaths.Count)];
            
            _freePaths.Remove(path);
            _usedPaths.Add(path);
            
            return path;
        }
        
        public void FreePath(EnemyMovementPathBehaviour path)
        {
            if (_usedPaths.Contains(path))
            {
                _usedPaths.Remove(path);
                
                if (_freePaths.Contains(path) == false)
                    _freePaths.Add(path);
            }
        }

        public bool HasFreePaths()
        {
            return _freePaths.Count > 0;
        }
        
        
        [ContextMenu("Initialize Paths")]
        private void UpdatePaths()
        {
            _paths = GetComponentsInChildren<EnemyMovementPathBehaviour>(true).ToList();
        }
    }
}