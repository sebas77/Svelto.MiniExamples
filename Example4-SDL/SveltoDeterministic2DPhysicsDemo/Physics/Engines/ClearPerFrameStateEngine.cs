using Svelto.Common;
using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents;

namespace SveltoDeterministic2DPhysicsDemo.Physics.Engines
{
    [Sequenced(nameof(PhysicsEngineNames.ClearPerFrameStateEngine))]
    public class ClearPerFrameStateEngine : IQueryingEntitiesEngine, IScheduledPhysicsEngine
    {
        public ClearPerFrameStateEngine(IEngineScheduler engineScheduler) { _engineScheduler = engineScheduler; }

        public void Execute(ulong tick)
        {
            foreach (var ((manifolds, count), _) in entitiesDB.QueryEntities<CollisionManifoldEntityComponent>(
                GameGroups.RigidBodyTag.Groups))
                for (var i = 0; i < count; i++)
                    manifolds[i] = CollisionManifoldEntityComponent.Default;
        }

        public void Ready() { _engineScheduler.RegisterScheduledPhysicsEngine(this); }

        readonly IEngineScheduler _engineScheduler;
        public   EntitiesDB       entitiesDB { get; set; }

        public string Name => nameof(ClearPerFrameStateEngine);
    }
}