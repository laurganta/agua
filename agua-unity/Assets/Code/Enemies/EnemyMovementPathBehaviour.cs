using UnityEngine;

namespace CleverEdge
{
    public class EnemyMovementPathBehaviour : MonoBehaviour
    {
        [SerializeField] public Transform _pointA;
        [SerializeField] public Transform _pointB;
        
        public Vector3 GetPosition(float lerpRatio, int direction)
        {
            if (direction == 1)
            {
                return Vector3.Lerp(_pointA.position, _pointB.position, lerpRatio);
            }
            else
            {
                return Vector3.Lerp(_pointB.position, _pointA.position, lerpRatio);
            }
        }
    }
}