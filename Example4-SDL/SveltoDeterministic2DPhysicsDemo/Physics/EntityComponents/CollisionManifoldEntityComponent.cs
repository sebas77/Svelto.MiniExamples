using System;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using Svelto.Common;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct CollisionManifoldEntityComponent : IEntityComponent, IDisposable
    {
        public CollisionManifoldEntityComponent(uint size)
        {
            Collisions = new NativeDynamicArrayCast<CollisionManifold>(NativeDynamicArray.Alloc<CollisionManifold>(Allocator.Temp, size));
        }

        public void Dispose()
        {
            Collisions.Dispose();
        }

        public NativeDynamicArrayCast<CollisionManifold> Collisions { get; set; }
    }
}