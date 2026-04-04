using UnityEngine;

namespace CleverEdge
{
    public class RotationAnimatorBehaviour : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 100f;
        [SerializeField] private Vector3 _rotationAxis = Vector3.up;
        
        private void Update()
        {
            transform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime);
        }
    }
}