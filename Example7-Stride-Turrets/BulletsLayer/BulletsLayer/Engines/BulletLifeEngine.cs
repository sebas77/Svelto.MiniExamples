using Svelto.Common.Internal;

namespace Svelto.ECS.MiniExamples.Turrets.BulletLayer
{
    /// <summary>
    /// Iterate all the bullets and remove them if they are under the floor
    /// </summary>
    class BulletLifeEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public BulletLifeEngine(IEntityFunctions entityFunctions) { _entityFunctions = entityFunctions; }
        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }

        public string name => this.TypeName();

        public void Step(in float deltaTime)
        {
            foreach (var ((position, egids, count), fromGroup) in entitiesDB.QueryEntities<PositionComponent>(
                BulletTag.Groups))
                for (var i = 0; i < count; i++)
                    if (position[i].position.Y < -0.01f)
                        _entityFunctions.RemoveEntity<BulletEntityDescriptor>(new EGID(egids[i], fromGroup));
        }

        readonly IEntityFunctions _entityFunctions;
    }
}