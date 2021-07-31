using System.Collections;
using Svelto.Common;
using Svelto.ECS.Example.Survive.Player.Gun;

namespace Svelto.ECS.Example.Survive.HUD
{
    [Sequenced(nameof(EnginesNames.UpdateAmmoEngine))]
    public class UpdateAmmoEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _listenForEnemyDeath = ListenForEnemyDeath();
        }
        
        public void   Step() { _listenForEnemyDeath.MoveNext(); }
        public string name   => nameof(UpdateAmmoEngine);

        IEnumerator ListenForEnemyDeath()
        {
            var consumer = _consumerFactory.GenerateConsumer<GunAttributesComponent>("GunFireConsumer2", 1);

            while (entitiesDB.HasAny<HUDEntityViewComponent>(ECSGroups.GUICanvas) == false)
                yield return null;
            
            var hudEntityView = entitiesDB.QueryUniqueEntity<HUDEntityViewComponent>(ECSGroups.GUICanvas);
            
            while (true)
            {
                while (consumer.TryDequeue(out _, out var egid))
                {
                    var gunComponent = entitiesDB.QueryEntity<GunAttributesComponent>(egid);
                    hudEntityView.ammoComponent.ammo = gunComponent.ammo;
                }

                yield return null;
            }
        }

        public UpdateAmmoEngine(IEntityStreamConsumerFactory consumerFactory) { _consumerFactory = consumerFactory; }
        
        readonly IEntityStreamConsumerFactory _consumerFactory;
        IEnumerator                           _listenForEnemyDeath;
    }
}