using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public struct RigidBodyComponent: IEntityComponent
    {
        public bool isKinematic;
        public Vector3 velocity;
    }
}