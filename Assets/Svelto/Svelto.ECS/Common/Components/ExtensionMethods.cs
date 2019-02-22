#if UNITY_2018_3_OR_NEWER
using UnityEngine;

namespace Svelto.ECS.Components.Unity
{
    namespace Svelto.ECS.Components
    {
        public static class ExtensionMethods
        {
            public static Vector3 ToVector3(this ECSVector3 vector) { return new Vector3(vector.x, vector.y, vector.z); }
        }
    }
}
#endif