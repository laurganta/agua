using System.Collections.Generic;
using UnityEngine;

namespace CleverEdge
{
    
    public class CameraShakeBehaviour : MonoBehaviour
    {
        [Header("Shake Settings")]
        [SerializeField] private AnimationCurve strengthOverLifetime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private bool useUnscaledTime = false;

        private readonly List<ShakeInstance> _activeShakes = new();
        private Vector3 _initialLocalPosition;

        private void Awake()
        {
            _initialLocalPosition = transform.localPosition;
            
            ServiceLocator.AddInstance(this);
        }

        private void OnEnable()
        {
            _initialLocalPosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            var deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (_activeShakes.Count == 0)
            {
                transform.localPosition = _initialLocalPosition;
                return;
            }

            var totalMagnitude = 0f;

            for (var i = _activeShakes.Count - 1; i >= 0; i--)
            {
                var shake = _activeShakes[i];
                shake.Elapsed += deltaTime;

                if (shake.Elapsed >= shake.Duration)
                {
                    _activeShakes.RemoveAt(i);
                    continue;
                }

                var normalizedTime = Mathf.Clamp01(shake.Elapsed / shake.Duration);
                var strength01 = strengthOverLifetime.Evaluate(normalizedTime);
                totalMagnitude += shake.BaseMagnitude * strength01;

                _activeShakes[i] = shake;
            }

            if (_activeShakes.Count == 0 || totalMagnitude <= 0f)
            {
                transform.localPosition = _initialLocalPosition;
                return;
            }

            var offset = Random.insideUnitSphere * totalMagnitude;
            transform.localPosition = _initialLocalPosition + offset;
        }

        public void Shake(float durationSeconds, float magnitude)
        {
            if (durationSeconds <= 0f || magnitude <= 0f)
                return;

            _activeShakes.Add(new ShakeInstance
            {
                Duration = durationSeconds,
                BaseMagnitude = magnitude,
                Elapsed = 0f
            });
        }

        public void StopAllShakes(bool resetPosition = true)
        {
            _activeShakes.Clear();

            if (resetPosition)
                transform.localPosition = _initialLocalPosition;
        }

        private struct ShakeInstance
        {
            public float Duration;
            public float BaseMagnitude;
            public float Elapsed;
        }
    }
}