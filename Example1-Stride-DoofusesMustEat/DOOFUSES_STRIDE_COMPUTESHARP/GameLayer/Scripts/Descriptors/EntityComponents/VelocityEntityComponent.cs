using System.Runtime.InteropServices;
using Stride.Core.Mathematics;

namespace Svelto.ECS.EntityComponents
{
    public struct ComputeVelocityComponent : IEntityComputeSharpComponent
    {
        public Vector3 velocity;
    }
}  