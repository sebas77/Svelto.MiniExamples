using FixedMaths;
using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class ClearPerFrameStateEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public ClearPerFrameStateEngine(IEngineScheduler engineScheduler) { _engineScheduler = engineScheduler; }

        public void Execute(FixedPoint delta)
        {
            foreach (var ((manifolds, count), _) in entitiesDB.QueryEntities<CollisionManifoldEntityComponent>(
                GameGroups.RigidBody.Groups))
                for (var i = 0; i < count; i++)
                    manifolds[i] = default;
        }

        public void Ready() { _engineScheduler.RegisterScheduledPhysicsEngine(this); }

        readonly IEngineScheduler _engineScheduler;
        public   EntitiesDB       entitiesDB { get; set; }

        public string Name => nameof(ClearPerFrameStateEngine);
    }
}