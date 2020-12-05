using Svelto.Common;
using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Maths;
using SveltoDeterministic2DPhysicsDemo.Physics.CollisionStructures;
using SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents;
using SveltoDeterministic2DPhysicsDemo.Physics.Types;

namespace SveltoDeterministic2DPhysicsDemo.Physics.Engines
{
    [Sequenced(nameof(PhysicsEngineNames.DetectBoxVsBoxCollisionsEngine))]
    public class DetectBoxVsBoxCollisionsEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public DetectBoxVsBoxCollisionsEngine(IEngineScheduler engineScheduler) { _engineScheduler = engineScheduler; }

        public void Execute(ulong tick)
        {
            foreach (var ((transforms, colliders, manifolds, rigidbodies, count), _) in entitiesDB
               .QueryEntities<TransformEntityComponent, BoxColliderEntityComponent, CollisionManifoldEntityComponent,
                    RigidbodyEntityComponent>(GameGroups.RigidBodyWithBoxColliders.Groups))
                for (var a = 0; a < count; a++)
                {
                    ref var colliderA  = ref colliders[a];
                    ref var transformA = ref transforms[a];
                    ref var rigidBodyA = ref rigidbodies[a];

                    var aabbA = colliderA.ToAABB(transformA.Position);

                    for (var b = a + 1; b < count; b++)
                    {
                        ref var colliderB  = ref colliders[b];
                        ref var transformB = ref transforms[b];
                        ref var rigidBodyB = ref rigidbodies[b];

                        if (rigidBodyA.IsKinematic && rigidBodyB.IsKinematic)
                            continue;

                        var aabbB = colliderB.ToAABB(transformB.Position);

                        var manifold = CalculateManifold(a, aabbA, b, aabbB);

                        if (!manifold.HasValue)
                            continue;

                        manifolds[a] = CollisionManifoldEntityComponent.From(manifold.Value);
                        //manifoldB = CollisionManifoldEntityComponent.From(manifold.Value.Reverse());
                    }
                }
        }

        public   void             Ready() { _engineScheduler.RegisterScheduledPhysicsEngine(this); }
        readonly IEngineScheduler _engineScheduler;
        public   EntitiesDB       entitiesDB { get; set; }

        public string Name => nameof(DetectBoxVsBoxCollisionsEngine);

        static CollisionManifold? CalculateManifold(int indexA, AABB a, int indexB, AABB b)
        {
            // First, calculate the Minkowski difference. a maps to red, and b maps to blue from our example (though it doesn't matter!)
            var top    = a.Max.Y - b.Min.Y;
            var bottom = a.Min.Y - b.Max.Y;
            var left   = a.Min.X - b.Max.X;
            var right  = a.Max.X - b.Min.X;

            // If the Minkowski difference intersects the origin, there's a collision
            if (right < FixedPoint.Zero || left > FixedPoint.Zero || top < FixedPoint.Zero || bottom > FixedPoint.Zero)
                return null;

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
                return CollisionManifold.From(min, penetration.Value.Normalize(), CollisionType.AABBToAABB, indexA
                                            , indexB);

            return null;
        }
    }
}