using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction
{
    class TransformImplementor:MonoBehaviour, ITransformImplementor
    {
        public Vector3 position
        {
            get => this.transform.position;
            set => this.transform.position = value;
        }

        public ValueReference<TransformImplementor> parent
        {
            set => transform.SetParent(((TransformImplementor) value).transform, false);
        }
    }

    interface ITransformImplementor : IImplementor
    {
        Vector3                              position { get; set; }
        ValueReference<TransformImplementor> parent   { set; }
    }
    
    struct TransformViewComponent : IEntityViewComponent
    {
        public ITransformImplementor transform;
        public EGID                  ID { get; set; }
    }
}