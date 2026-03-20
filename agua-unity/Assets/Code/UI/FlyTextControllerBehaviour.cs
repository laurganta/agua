using UnityEngine;
using UnityEngine.Pool;

namespace CleverEdge
{
    public class FlyTextControllerBehaviour : MonoBehaviour
    {
        [SerializeField] private FlyTextBehaviour _scoreFlyTextPrefab;
        [SerializeField] private Vector2 _scoreSizeReferenceRange;
        [SerializeField] private Vector3 _spawnOffset;

        private ObjectPool<FlyTextBehaviour> _flyTextPool;

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            _flyTextPool = new ObjectPool<FlyTextBehaviour>(
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

        public void SpawnFlyText(Vector3 position, string text, Color color, float scale = 1)
        {
            if (_flyTextPool == null)
                InitializePool();
            
            var scoreFlyText = _flyTextPool.Get();
            scoreFlyText.transform.position = position + _spawnOffset;
            scoreFlyText.transform.localScale = Vector3.one * scale;
            scoreFlyText.SetText(text, color);
            scoreFlyText.PlayAnimation((flyText) => _flyTextPool.Release(flyText));
        }
    }
}