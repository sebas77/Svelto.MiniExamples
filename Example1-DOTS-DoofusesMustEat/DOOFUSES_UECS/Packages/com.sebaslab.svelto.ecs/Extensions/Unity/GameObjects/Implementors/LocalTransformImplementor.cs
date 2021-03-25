#if UNITY_5 || UNITY_5_3_OR_NEWER
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    [DisallowMultipleComponent]
    public class LocalTransformImplementor : MonoBehaviour, ILocalTransformImplementor
    {
        public Vector3    position { get => transform.localPosition; set => transform.localPosition = value; }
        public Quaternion rotation { get => transform.localRotation; set => transform.localRotation = value; }
        public Vector3    scale    { get => transform.localScale;    set => transform.localScale = value; }
    }

    public interface ILocalTransformImplementor : IImplementor
    {
        Vector3    position { get; set; }
        Quaternion rotation { get; set; }
        Vector3    scale    { get; set; }
    }
    
    public struct LocalTransformViewComponent : IEntityViewComponent
    {
        public ILocalTransformImplementor localTransform;
        public EGID                     ID { get; set; }
    }

}
#endif