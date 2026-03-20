using UnityEngine;
using UnityEngine.Pool;

namespace CleverEdge
{
    public class ScoreFlyTextControllerBehaviour : MonoBehaviour
    {
        [SerializeField] private ScoreFlyTextBehaviour _scoreFlyTextPrefab;
        [SerializeField] private Vector2 _scoreSizeRange;
        [SerializeField] private Vector2 _scoreSizeReferenceRange;
        [SerializeField] private Vector3 _spawnOffset;

        private ObjectPool<ScoreFlyTextBehaviour> _scoreFlyTextPool;

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            _scoreFlyTextPool = new ObjectPool<ScoreFlyTextBehaviour>(
                createFunc: () =>
                {
                    var flyText = Instantiate(_scoreFlyTextPrefab, transform.position, Quaternion.Euler(45, 0, 0));
                    flyText.transform.SetParent(transform);
                    flyText.gameObject.SetActive(false);
                    return flyText;
                },
                actionOnGet: (obj) => obj.gameObject.SetActive(true),
                actionOnRelease: (obj) => obj.gameObject.SetActive(false),
                actionOnDestroy: (obj) =>
                {
                    if (this != null && this.Equals(null) == false)
                        Destroy(obj.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 20
            );
        }

        public void SpawnScoreFlyText(Vector3 position, int score, Color color)
        {
            if (_scoreFlyTextPool == null)
            {
                InitializePool();
            }
            
            var scoreFlyText = _scoreFlyTextPool.Get();
            scoreFlyText.transform.position = position + _spawnOffset;
            scoreFlyText.transform.localScale = Vector3.one * Mathf.Lerp(_scoreSizeRange.x, _scoreSizeRange.y, 
                Mathf.InverseLerp(_scoreSizeReferenceRange.x, _scoreSizeReferenceRange.y, score));
            scoreFlyText.SetScore(score, color);
            scoreFlyText.PlayAnimation((flyText) => _scoreFlyTextPool.Release(flyText));
        }
    }
}