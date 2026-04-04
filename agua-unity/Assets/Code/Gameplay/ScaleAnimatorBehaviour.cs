using UnityEngine;
using Random = UnityEngine.Random;

namespace CleverEdge
{
    
    public class ScaleAnimatorBehaviour : MonoBehaviour
    {
        [SerializeField] private float _delta;
        [SerializeField] private Vector2 _intervalRange;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private Vector2 _delayRange;
        [SerializeField] private bool _lockYAxis = true;

        private float _currentDuration;
        private float _timer;
        private int _direction;

        private Vector3 _scaleMin;
        private Vector3 _scaleMax;
        
        private void Awake()
        {
            _scaleMin = transform.localScale - Vector3.one * _delta;
            _scaleMax = transform.localScale + Vector3.one * _delta;

            if (_lockYAxis == true)
            {
                _scaleMin.y = transform.localScale.y;
                _scaleMax.y = transform.localScale.y;
            }

            _currentDuration = Random.Range(_intervalRange.x, _intervalRange.y);
            
            _direction = Random.value < 0.5f ? -1 : 1;
            transform.localScale = _direction == 1 ? _scaleMin : _scaleMax;
            
            var delay = Random.Range(_delayRange.x, _delayRange.y);
            _timer = -delay;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _currentDuration)
            {
                _timer = 0f;
                _direction *= -1;
                _currentDuration = Random.Range(_intervalRange.x, _intervalRange.y);
            }

            var t = _timer / _currentDuration;
            var curveValue = _curve.Evaluate(t);
            
            transform.localScale = Vector3.Lerp(_direction == 1 ? _scaleMin : _scaleMax, 
                _direction == 1 ? _scaleMax : _scaleMin, curveValue);
            
        }
    }
}