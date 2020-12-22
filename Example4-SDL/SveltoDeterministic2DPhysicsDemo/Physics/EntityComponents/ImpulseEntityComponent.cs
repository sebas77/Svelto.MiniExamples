using FixedMaths;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct ImpulseEntityComponent : IEntityComponent
    {
        public FixedPointVector2 Impulse { get; set; }

        public ImpulseEntityComponent(FixedPointVector2 impulse)
        {
            Impulse = impulse;
        }
    }
}