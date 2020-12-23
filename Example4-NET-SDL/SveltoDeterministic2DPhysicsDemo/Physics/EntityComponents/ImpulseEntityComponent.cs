using System;
using FixedMaths;
using Svelto.Common;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct ImpulseEntityComponent : IEntityComponent, IDisposable
    {
        public ImpulseEntityComponent(uint size)
        {
            Impulses = new NativeDynamicArrayCast<FixedPointVector2>(NativeDynamicArray.Alloc<FixedPointVector2>(Allocator.Persistent, size));
        }

        public void Dispose()
        {
            Impulses.Dispose();
        }

        public NativeDynamicArrayCast<FixedPointVector2> Impulses { get; set; }
    }
}