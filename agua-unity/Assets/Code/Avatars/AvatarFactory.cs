using UnityEngine;

namespace CleverEdge
{
    public static class AvatarFactory
    {
        public static GameObject CreatePrefab(int prefabIndex, Transform parent)
        {
            var prefab = ServiceLocator.GetInstance<AvatarsConfig>().GetAvatarPrefab(prefabIndex);
            
            var avatar = Object.Instantiate(prefab, parent);
            avatar.transform.localPosition = Vector3.zero;
            avatar.transform.localRotation = Quaternion.identity;
            avatar.transform.localScale = Vector3.one;

            return avatar;
        }
    }
}