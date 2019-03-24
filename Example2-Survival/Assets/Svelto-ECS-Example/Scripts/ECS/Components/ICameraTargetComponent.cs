using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    public interface ICameraTargetComponent
    {
        Vector3 position { get; }
    }
}