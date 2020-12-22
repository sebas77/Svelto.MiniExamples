using UnityEngine;

namespace Boxtopia.Utilities
{
    public static class TransformExtensions
    {
        public static Transform FindDeepChild(this Transform transform, string name)
        {
            if (transform.name == name)
            {
                return transform;
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var result = child.FindDeepChild(name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
