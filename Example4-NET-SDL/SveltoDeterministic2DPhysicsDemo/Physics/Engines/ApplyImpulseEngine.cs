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
            var entities =
                entitiesDB.QueryEntities<ImpulseEntityComponent, RigidbodyEntityComponent>(GameGroups.DynamicRigidBodies.Groups);

            foreach (var ((impulses, rigidBodies, count), _) in entities)
            {
                for (var i = 0; i < count; i++)
                {
                    ref var impulseComponent = ref impulses[i];
                    ref var rigidbody = ref rigidBodies[i];

                    for (var j = 0; j < impulseComponent.Impulses.count; j++)
                    {
                        var impulse = impulseComponent.Impulses[j];

                        rigidbody.Direction = (rigidbody.Velocity - impulse).Normalize();
                    }
                }
            }
        }

        public string Name => nameof(ApplyImpulseEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}