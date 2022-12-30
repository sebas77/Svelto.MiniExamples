using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public struct RotationComponent: IEntityComponent
    {
        public Quaternion rotation;        
    }
}