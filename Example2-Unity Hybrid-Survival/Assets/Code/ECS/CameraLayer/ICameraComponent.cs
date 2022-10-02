using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    public interface ICameraComponent
    {
        Ray     camRay      { get; }
        Vector3 camRayInput { set; }
        Vector3 offset      { get; }
    }
}