using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public interface IPositionComponent
    {
        Vector3 position { get; }
    }
}