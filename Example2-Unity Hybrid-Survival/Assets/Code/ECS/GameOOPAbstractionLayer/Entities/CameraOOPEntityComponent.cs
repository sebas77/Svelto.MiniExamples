using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public struct CameraOOPEntityComponent : IEntityComponent
    {
        public Vector3 offset;
        public Vector3 camRayInput;
        public Ray camRay;
        public bool inputRead;
    }
}