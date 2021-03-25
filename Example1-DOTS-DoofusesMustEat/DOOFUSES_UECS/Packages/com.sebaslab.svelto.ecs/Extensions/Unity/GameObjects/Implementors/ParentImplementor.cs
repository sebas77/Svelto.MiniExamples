#if UNITY_5 || UNITY_5_3_OR_NEWER
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IParentImplementor : IImplementor
    {
        ValueReference<IParentImplementor> reference { get;  set; }
    }

    [DisallowMultipleComponent]
    public class ParentImplementor : MonoBehaviour, IParentImplementor
    {
        public ValueReference<IParentImplementor> reference
        {
            get => new ValueReference<IParentImplementor>(this);
            set => transform.SetParent((((ParentImplementor) value).transform), false);
        }
    }

    public struct ParentViewComponent : IEntityViewComponent
    {
        public IParentImplementor parent;
        public EGID             ID { get; set; }
    }
}
#endif