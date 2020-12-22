using System;
using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class ApplyImpulseEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public void Execute(FixedPoint delta)
        {
            foreach (var ((impulses, referenceEgids, count), _)
                in entitiesDB.QueryEntities<ImpulseEntityComponent, ReferenceEgidComponent>(GameGroups.InCollision.Groups))
            {
                for (var i = 0; i < count; i++)
                {
                    ref var impulse = ref impulses[i];
                    ref var referenceEgid = ref referenceEgids[i];

                    ref var rigidbody = ref entitiesDB.QueryEntity<RigidbodyEntityComponent>(referenceEgid.Egid);

                    rigidbody.Direction = (rigidbody.Velocity - impulse.Impulse).Normalize();
                }
            }
        }

        public string Name => nameof(ApplyImpulseEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}