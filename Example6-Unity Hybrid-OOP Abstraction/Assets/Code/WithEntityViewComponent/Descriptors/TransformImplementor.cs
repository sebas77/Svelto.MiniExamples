using System;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.EntityViewComponents
{
    interface ITransformImplementor : IImplementor
    {
        Vector3                               position { get; set; }
        //ValueReference is the only way to store a reference inside an Implementor
        ValueReference<ITransformImplementor> parent   { set; }
    }

    class TransformImplementor : MonoBehaviour, ITransformImplementor
    {
        public Vector3 position { get => transform.localPosition; set => transform.localPosition = value; }

        public ValueReference<ITransformImplementor> parent
        {
            //convert back the ValueReference to the real implementor
            set => transform.SetParent((value.ConvertAndDispose(this) as TransformImplementor).transform, false);
        }
    }
}