using FixedMaths;
using MiniExamples.DeterministicPhysicDemo.Physics.Descriptors;
using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Engines
{
    public class ClearPerFrameStateEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public ClearPerFrameStateEngine(IEntityFunctions entityFunctions)
        {
            _entityFunctions = entityFunctions;
        }

        public void Execute(FixedPoint delta)
        {
            foreach (var ((egids, count), _) in entitiesDB.QueryEntities<EGIDComponent>(GameGroups.InCollision.Groups))
                for (var i = 0; i < count; i++)
                    _entityFunctions.RemoveEntity<CollisionDescriptor>(egids[i].ID);
        }

        readonly IEntityFunctions _entityFunctions;

        public string Name => nameof(ClearPerFrameStateEngine);
        public EntitiesDB entitiesDB { get; set; }
        public void Ready() { }
    }
}