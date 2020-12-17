using Svelto.ECS;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.CollisionStructures;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using MiniExamples.DeterministicPhysicDemo.Physics.Types;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class DetectBoxVsBoxCollisionsEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public DetectBoxVsBoxCollisionsEngine(IEngineScheduler engineScheduler) { _engineScheduler = engineScheduler; }

        public void Execute(FixedPoint delta)
        {
            var dynamicEntities = new DoubleEntitiesEnumerator<TransformEntityComponent, BoxColliderEntityComponent, 
                CollisionManifoldEntityComponent>(
                    entitiesDB.QueryEntities<TransformEntityComponent, BoxColliderEntityComponent,
                            CollisionManifoldEntityComponent>(GameGroups.DynamicRigidBodyWithBoxColliders.Groups));

            foreach (var ((transformsA, collidersA, buffer3, count), indexA, ((transformsB, collidersB, buffer4, i), exclusiveGroupStruct),
                indexB) in dynamicEntities)
            {
                ref var colliderA  = ref collidersA[indexA];
                ref var transformA = ref transformsA[indexA];

                var aabbA = colliderA.ToAABB(transformA.Position);

                ref var colliderB  = ref collidersB[indexB];
                ref var transformB = ref transformsB[indexB];

                var aabbB = colliderB.ToAABB(transformB.Position);

                var manifold = CalculateManifold(indexA, aabbA, indexB, aabbB);
            }
        }

        public   void             Ready() { _engineScheduler.RegisterScheduledPhysicsEngine(this); }
        readonly IEngineScheduler _engineScheduler;
        public   EntitiesDB       entitiesDB { get; set; }

        public string Name => nameof(DetectBoxVsBoxCollisionsEngine);

        static bool CalculateManifold
        (int indexA, AABB a, int indexB, AABB b)
        {
            // First, calculate the Minkowski difference. a maps to red, and b maps to blue from our example (though it doesn't matter!)
            var top    = a.Max.Y - b.Min.Y;
            var bottom = a.Min.Y - b.Max.Y;
            var left   = a.Min.X - b.Max.X;
            var right  = a.Max.X - b.Min.X;

            // If the Minkowski difference intersects the origin, there's a collision
            if (right < FixedPoint.Zero || left > FixedPoint.Zero || top < FixedPoint.Zero || bottom > FixedPoint.Zero)
                return false;

            // The pen vector is the shortest vector from the origin of the MD to an edge.
            // You know this has to be a vertical or horizontal line from the origin (these are by def. the shortest)
            var                min         = FixedPoint.MaxValue;
            FixedPointVector2? penetration = null;

            if (MathFixedPoint.Abs(left) < min)
            {
                min         = MathFixedPoint.Abs(left);
                penetration = FixedPointVector2.From(left, FixedPoint.Zero);
            }

            if (MathFixedPoint.Abs(right) < min)
            {
                min         = MathFixedPoint.Abs(right);
                penetration = FixedPointVector2.From(right, FixedPoint.Zero);
            }

            if (MathFixedPoint.Abs(top) < min)
            {
                min         = MathFixedPoint.Abs(top);
                penetration = FixedPointVector2.From(FixedPoint.Zero, top);
            }

            if (MathFixedPoint.Abs(bottom) < min)
            {
                min         = MathFixedPoint.Abs(bottom);
                penetration = FixedPointVector2.From(FixedPoint.Zero, bottom);
            }

            if (penetration.HasValue)
            {
                FixedPointVector2 normal = penetration.Value.Normalize();
                return true;
            }

            return false;
        }
    }
}