using UnityEngine;

namespace CleverEdge
{
    public static class Extensions
    {
        public static void ClearChildren(this Transform target)
        {
            var childCount = target.childCount;
            for (var index = childCount - 1; index >= 0; index--)
            {
                var child = target.GetChild(index);
                Object.Destroy(child.gameObject);
            }
        }
    }
}