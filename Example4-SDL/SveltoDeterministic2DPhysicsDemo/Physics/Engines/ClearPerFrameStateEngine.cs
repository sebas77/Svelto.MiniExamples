using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class ClearPerFrameStateEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public void Execute(FixedPoint delta)
        {
            var entities = entitiesDB.QueryEntities<CollisionManifoldEntityComponent, ImpulseEntityComponent>(GameGroups.DynamicRigidBodies.Groups);

            foreach (var ((collisions, impulses, count), _) in entities)
            {
                for (var i = 0; i < count; i++)
                {
                    collisions[i].Collisions.Clear();
                    impulses[i].Impulses.Clear();
                }
            }
        }

        public string Name => nameof(ClearPerFrameStateEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}