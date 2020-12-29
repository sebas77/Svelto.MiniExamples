using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public struct TransformComponent : IEntityComponent
    {
        public Vector3 position;

        public TransformComponent(Vector3 vector3) { position = vector3; }
    }
}