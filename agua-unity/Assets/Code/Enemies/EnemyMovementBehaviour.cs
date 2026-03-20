using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CleverEdge
{
    public class EnemyMovementBehaviour : MonoBehaviour
    {
        [SerializeField] private Vector2 _durationRange;
        [SerializeField] private AnimationCurve _animationCurve;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _animationSpeed;

        private Coroutine _followPathCoroutine;
        
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        
        public EnemyMovementPathBehaviour CurrentPath { get; private set; }
        
        public Vector3 SetPath(EnemyMovementPathBehaviour path, Action onPathComplete)
        {
            CurrentPath = path;
            
            var direction = Random.value < 0.5f ? 1 : -1;
            
            _followPathCoroutine = StartCoroutine(FollowPath(path, direction, onPathComplete));

            return path.GetPosition(0, direction);
        }

        public void HideAfterSeconds(float seconds, Action onHide)
        {
            StartCoroutine(HideAfterSecondsCoroutine(seconds, onHide));
        }
        
        private IEnumerator HideAfterSecondsCoroutine(float seconds, Action onHide)
        {
            yield return new WaitForSeconds(seconds);
            onHide?.Invoke();
        }

        private IEnumerator FollowPath(EnemyMovementPathBehaviour path, int direction, Action onPathComplete)
        {
            var t = 0f;
            var duration = Random.Range(_durationRange.x, _durationRange.y);
            
            _animator.SetFloat(SpeedHash, _animationSpeed);
            
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                var ratio = _animationCurve.Evaluate(t);
                var position = path.GetPosition(ratio, direction);
                transform.position = position;
                yield return null;
            }
            
            onPathComplete?.Invoke();
        }

        public void Stop()
        {
            if (_followPathCoroutine != null)
                StopCoroutine(_followPathCoroutine);
        }
    }
}