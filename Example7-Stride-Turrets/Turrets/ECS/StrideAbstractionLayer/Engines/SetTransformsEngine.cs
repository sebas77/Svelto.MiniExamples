using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class SetTransformsEngine : SyncScript, IQueryingEntitiesEngine
    {
        public SetTransformsEngine(ECSStrideEntityManager ecsStrideEntityManager)
        {
            _ECSStrideEntityManager = ecsStrideEntityManager;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public override void Update()
        {
            var groups = entitiesDB.FindGroups<EGIDComponent, MatrixComponent>();
            foreach (var ((egids, transforms, count), _) in
                entitiesDB.QueryEntities<EGIDComponent, MatrixComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    var transformComponent = _ECSStrideEntityManager.GetStrideEntity(egids[i].ID.entityID).Transform;
                    transformComponent.WorldMatrix = transforms[i].matrix;
                    transformComponent.UpdateLocalFromWorld();
                }
            }
        }

        readonly ECSStrideEntityManager _ECSStrideEntityManager;
    }
}