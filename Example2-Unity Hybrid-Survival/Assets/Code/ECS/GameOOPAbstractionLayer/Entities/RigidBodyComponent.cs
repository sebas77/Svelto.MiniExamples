using UnityEngine;

namespace Svelto.ECS.Example.Survive.Transformable
{
    public struct RigidBodyComponent: IEntityComponent
    {
        public bool isKinematic;
        public Vector3 velocity;
    }
}