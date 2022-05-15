using Stride.Core.Mathematics;

namespace Svelto.ECS.EntityComponents
{
    public struct VelocityEntityComponent : 
        //IEntityComputeSharpComponent
    IEntityComponent
    {
        public Vector3 velocity;
    }
}  