#if UNITY_5 || UNITY_5_3_OR_NEWER
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IEGIDHolder
    {
        EGID ID { set; }
    }
    [DisallowMultipleComponent]
    public class EGIDHolderImplementor : MonoBehaviour, IEGIDHolder, IImplementor
    {
        public EGID ID { get; set; }
    }
}
#endif