using UnityEngine;

namespace Svelto.ECS.Example.Survive.Transformable
{
    public struct RotationComponent: IEntityComponent
    {
        public Quaternion rotation;        
    }
}