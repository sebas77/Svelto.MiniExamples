using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class BulletLifeEngine : SyncScript, IQueryingEntitiesEngine
    {
        public BulletLifeEngine(IEntityFunctions entityFunctions) { _entityFunctions = entityFunctions; }
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }

        public override void Update()
        {
            foreach (var ((position, egids, count), _) in entitiesDB.QueryEntities<PositionComponent, EGIDComponent>(
                BulletTag.Groups))
                for (var i = 0; i < count; i++)
                    if (position[i].position.Y < -0.01f)
                        _entityFunctions.RemoveEntity<BulletEntityDescriptor>(egids[i].ID);
        }

        readonly IEntityFunctions _entityFunctions;
    }
}