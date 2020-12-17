using Svelto.ECS;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class ApplyVelocityEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public ApplyVelocityEngine(IEngineScheduler engineScheduler)
        {
            _engineScheduler             = engineScheduler;
        }

        public void Execute(FixedPoint delta)
        {
            foreach (var ((rigidbodies, transforms, count), _) in entitiesDB
               .QueryEntities<RigidbodyEntityComponent, TransformEntityComponent>(GameGroups.RigidBodies.Groups))
                for (var i = 0; i < count; i++)
                {
                    ref var rigidbody = ref rigidbodies[i];

                    if (rigidbody.IsKinematic)
                        continue;
                    
                    ref var position = ref transforms[i].Position;
                    
                    FixedPointVector2 velocity       = rigidbody.Direction * rigidbody.Speed;
                    var               targetPosition = position + velocity / delta;
                    
                    transforms[i]      = TransformEntityComponent.From(targetPosition, position);
                    rigidbody.Velocity = velocity;
                }
        }

        public void Ready()
        {
            _engineScheduler.RegisterScheduledPhysicsEngine(this);
        }

        public   EntitiesDB       entitiesDB { get; set; }
        public   string           Name => nameof(ApplyVelocityEngine);
        
        readonly IEngineScheduler _engineScheduler;
    }
}