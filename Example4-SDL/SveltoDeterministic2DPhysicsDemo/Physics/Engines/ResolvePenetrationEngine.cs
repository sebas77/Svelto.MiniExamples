using Svelto.ECS;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class ResolvePenetrationEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public ResolvePenetrationEngine(IEngineScheduler engineScheduler) { _engineScheduler = engineScheduler; }

        public void Execute(FixedPoint delta)
        {
            foreach (var ((transforms, rigidbodies, manifolds, count), _) in entitiesDB
               .QueryEntities<TransformEntityComponent, RigidbodyEntityComponent, CollisionManifoldEntityComponent>(
                    GameGroups.RigidBodies.Groups))
                for (var i = 0; i < count; i++)
                {
                    ref var transform = ref transforms[i];
                    ref var manifold  = ref manifolds[i];
                    ref var rigidbody = ref rigidbodies[i];

                    if (rigidbody.IsKinematic)
                        continue;

                    if (!manifold.CollisionManifold.HasValue)
                        continue;

                    var collisionManifold = manifold.CollisionManifold.Value;

                    transform = TransformEntityComponent.From(transform.Position - collisionManifold.Normal
                                                            , transform.PositionLastPhysicsTick
                                                            , transform.Position - collisionManifold.Normal
                                                            / FixedPoint.Two);
                }
        }

        public void Ready() { _engineScheduler.RegisterScheduledPhysicsEngine(this); }

        readonly IEngineScheduler _engineScheduler;
        public   EntitiesDB       entitiesDB { get; set; }

        public string Name => nameof(ResolvePenetrationEngine);
    }
}