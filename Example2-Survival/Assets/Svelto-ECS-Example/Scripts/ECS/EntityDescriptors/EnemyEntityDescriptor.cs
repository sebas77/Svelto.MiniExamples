using Svelto.ECS.Example.Survive.Characters.Sounds;
using Svelto.ECS.Example.Survive.HUD;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyEntityDescriptor : IEntityDescriptor
    {
        static readonly IEntityBuilder[] _entitiesToBuild = {
                                                                new EntityBuilder <EnemyEntityStruct>(),
                                                                new EntityBuilder <EnemyEntityViewStruct>(),
                                                                new EntityBuilder <EnemyAttackEntityView>(),
                                                                new EntityBuilder <DamageSoundEntityView>(),
                                                                new EntityBuilder <EnemyAttackStruct>(),
                                                                new EntityBuilder <HealthEntityStruct>(),
                                                                new EntityBuilder <ScoreValueEntityStruct>(),
                                                                new EntityBuilder<EnemySinkStruct>(), 
                                                                new EntityBuilder <DamageableEntityStruct>()};
        public IEntityBuilder[] entitiesToBuild
        {
            get { return _entitiesToBuild; }
        }
    }
}