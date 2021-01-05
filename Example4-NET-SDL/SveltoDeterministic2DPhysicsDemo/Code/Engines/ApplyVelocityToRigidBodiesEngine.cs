using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class ApplyVelocityToRigidBodiesEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public void Execute(in FixedPoint delta)
        {
            foreach (var ((rigidbodies, transforms, count), _) in entitiesDB
               .QueryEntities<RigidbodyEntityComponent, TransformEntityComponent>(GameGroups.DynamicRigidBodies.Groups))
                for (var i = 0; i < count; i++)
                {
                    ref var rigidbody = ref rigidbodies[i];

                    ref var position = ref transforms[i].Position;

                    var velocity       = rigidbody.Direction * rigidbody.Speed;
                    var targetPosition = position + velocity / delta;

                    transforms[i].Position                = targetPosition;
                    transforms[i].PositionLastPhysicsTick = position;
                    transforms[i].HasMidPoint             = false;

                    rigidbody.Velocity = velocity;
                }
        }

        public string Name => nameof(ApplyVelocityToRigidBodiesEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}