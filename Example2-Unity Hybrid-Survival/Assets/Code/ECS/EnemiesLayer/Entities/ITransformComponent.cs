using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public interface ITransformComponent : IPositionComponent
    {
        new Vector3 position { set; }
        Quaternion  rotation { set; }
    }
}