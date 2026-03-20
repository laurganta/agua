using UnityEngine;

namespace CleverEdge
{
    public class AvatarBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform _avatarParent;
        
        public int AvatarIndex { get; private set; }
        
        public void SetAvatar(int avatarIndex)
        {
            AvatarIndex = avatarIndex;
            _avatarParent.ClearChildren();
            AvatarFactory.CreatePrefab(avatarIndex, _avatarParent);
        }
    }
}