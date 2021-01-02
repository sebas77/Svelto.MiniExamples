using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.EntityViewComponents
{
    interface ITransformImplementor : IImplementor
    {
        Vector3                               position { get; set; }
        ValueReference<ITransformImplementor> parent   { set; }
    }

    class TransformImplementor : MonoBehaviour, ITransformImplementor
    {
        public Vector3 position { get => transform.localPosition; set => transform.localPosition = value; }

        public ValueReference<ITransformImplementor> parent
        {
            set => transform.SetParent(((TransformImplementor) value).transform, false);
        }
    }
}