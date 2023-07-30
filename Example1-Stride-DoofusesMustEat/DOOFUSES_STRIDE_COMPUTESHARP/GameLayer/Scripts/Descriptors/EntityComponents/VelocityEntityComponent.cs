using System.Runtime.InteropServices;
using Stride.Core.Mathematics;

namespace Svelto.ECS.EntityComponents
{
    [StructLayout(LayoutKind.Auto, Pack = 32)] //does the gpu benefit from this?
    public struct ComputeVelocityComponent : IEntityComputeSharpComponent
    {
        public Vector3 velocity;
    }
}  