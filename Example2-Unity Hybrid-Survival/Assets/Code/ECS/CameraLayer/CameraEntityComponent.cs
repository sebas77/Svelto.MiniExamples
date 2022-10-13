using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    public struct CameraEntityComponent : IEntityComponent
    {
        public uint resourceID;
        
        public Vector3 offset;
        public Vector3 camRayInput;
        public Ray camRay;
    }
}