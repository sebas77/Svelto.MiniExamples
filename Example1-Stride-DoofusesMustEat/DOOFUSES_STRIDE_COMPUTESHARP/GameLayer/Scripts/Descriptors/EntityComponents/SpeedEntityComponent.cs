using System.Runtime.InteropServices;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    [StructLayout(LayoutKind.Auto, Pack = 32)] //does the gpu benefit from this?
    struct ComputeSpeedComponent : IEntityComputeSharpComponent
    {
        public float speed;
    }
}