using UnityEngine;

namespace CleverEdge
{
    public class CameraShakeBehaviour : MonoBehaviour
    {
        [SerializeField] private float _shakeDuration = 0.5f;
        [SerializeField] private float _shakeMagnitude = 0.1f;
        [SerializeField] private float _durationClamp;

        private Vector3 _initialPosition;
        private float _shakeTimer;

        private void Start()
        {
            _initialPosition = transform.localPosition;
        }
        
        public void TriggerShake()
        {
            Shake(_shakeDuration);
        }

        public void Shake(float duration)
        {
            _shakeTimer += duration;
            if (_shakeTimer > _durationClamp)
                _shakeTimer = _durationClamp;
        }

        private void Update()
        {
            if (_shakeTimer > 0)
            {
                var magnitude = _shakeMagnitude * (_shakeTimer / _shakeDuration);
                transform.localPosition = _initialPosition + Random.insideUnitSphere * magnitude;
                _shakeTimer -= Time.deltaTime;
            }
            else
            {
                transform.localPosition = _initialPosition;
            }
        }
    }
}