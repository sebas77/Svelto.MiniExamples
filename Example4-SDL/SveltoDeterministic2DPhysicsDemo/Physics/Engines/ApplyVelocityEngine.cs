using Svelto.Common;
using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Maths;
using SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents;

namespace SveltoDeterministic2DPhysicsDemo.Physics.Engines
{
    [Sequenced(nameof(PhysicsEngineNames.ApplyVelocityEngine))]
    public class ApplyVelocityEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public ApplyVelocityEngine(IEngineScheduler engineScheduler, FixedPoint physicsSimulationsPerSecond)
        {
            _physicsSimulationsPerSecond = physicsSimulationsPerSecond;
            _engineScheduler             = engineScheduler;
        }

        public void Execute(ulong tick)
        {
            foreach (var ((rigidbodies, transforms, count), _) in entitiesDB
               .QueryEntities<RigidbodyEntityComponent, TransformEntityComponent>(GameGroups.RigidBodies.Groups))
                for (var i = 0; i < count; i++)
                {
                    ref var transform = ref transforms[i];
                    ref var rigidbody = ref rigidbodies[i];

                    if (rigidbody.IsKinematic)
                        continue;

                    var position = transform.Position;
                    var velocity = rigidbody.Direction * rigidbody.Speed;

                    var targetPosition = position + velocity / _physicsSimulationsPerSecond;

                    transform = TransformEntityComponent.From(targetPosition, position);
                    rigidbody = rigidbody.CloneAndReplaceVelocity(velocity);
                }
        }

        public void Ready() { _engineScheduler.RegisterScheduledPhysicsEngine(this); }

        readonly IEngineScheduler _engineScheduler;
        readonly FixedPoint       _physicsSimulationsPerSecond;
        public   EntitiesDB       entitiesDB { get; set; }

        public string Name => nameof(ApplyVelocityEngine);
    }
}