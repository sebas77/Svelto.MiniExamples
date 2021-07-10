using Stride.Engine;
using Svelto.DataStructures;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class SetTransformsEngine : SyncScript, IQueryingEntitiesEngine
    {
        readonly ECSStrideEntityManager _ECSStrideEntityManager;

        public SetTransformsEngine(ECSStrideEntityManager ecsStrideEntityManager)
        {
            _ECSStrideEntityManager = ecsStrideEntityManager;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public override void Update()
        {
            //TODO EGIDCOMPONENT MUST BECOME STRIDEENTITYCOMPONENT
            LocalFasterReadOnlyList<ExclusiveGroupStruct> groups =
                entitiesDB.FindGroups<EGIDComponent, MatrixComponent>();
            foreach (var ((egids, transforms, count), _) in
                entitiesDB.QueryEntities<EGIDComponent, MatrixComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    _ECSStrideEntityManager.GetStrideEntity(egids[i].ID.entityID).Transform.LocalMatrix =
                        transforms[i].matrix;
                }
            }
        }
    }
}