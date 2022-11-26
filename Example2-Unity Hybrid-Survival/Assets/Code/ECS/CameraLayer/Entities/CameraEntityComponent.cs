using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    public struct CameraEntityComponent : Svelto.ECS.IEntityComponent
    {
        public uint resourceID;
        
        public Vector3 offset;
        public Vector3 camRayInput;
        public Ray camRay;
    }
}