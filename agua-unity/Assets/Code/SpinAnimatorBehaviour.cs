using UnityEngine;

namespace CleverEdge
{
    public class SpinAnimatorBehaviour : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _spinDuration;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            var t = _timer / _spinDuration;
            var curveValue = _curve.Evaluate(t);
            transform.localRotation = Quaternion.Euler(0, curveValue * 360, 0);

            if (_timer >= _spinDuration)
            {
                _timer = 0;
            }
        }
    }
}