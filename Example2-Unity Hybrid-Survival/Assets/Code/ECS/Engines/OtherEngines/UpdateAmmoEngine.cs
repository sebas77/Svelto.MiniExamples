using System.Collections;
using Svelto.Common;
using Svelto.ECS.Example.Survive.Player.Gun;
using Svelto.ECS.Example.Survive.Player;



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
            var consumer = _consumerFactory.GenerateConsumer<AmmoGunComponent>("GunAmmoConsumer1", 1);

            while (entitiesDB.HasAny<HUDEntityViewComponent>(ECSGroups.GUICanvas) == false)
                yield return null;
            
            var hudEntityView = entitiesDB.QueryUniqueEntity<HUDEntityViewComponent>(ECSGroups.GUICanvas);
            
            // Select first weapon to display to HUD
            while (entitiesDB.HasAny<PlayerWeaponComponent>(Player.Player.Groups[0]) == false)
                yield return null;

            var weapon = entitiesDB.QueryUniqueEntity<PlayerWeaponComponent>(Player.Player.Groups[0]);

            var gunEGID            = weapon.weapon.ToEGID(entitiesDB);
            var playerGunComponent = entitiesDB.QueryEntity<AmmoGunComponent>(gunEGID);
            hudEntityView.ammoComponent.ammo = playerGunComponent.ammo;

            while (true)
            {
                while (consumer.TryDequeue(out _, out var egid))
                {
                    playerGunComponent = entitiesDB.QueryEntity<AmmoGunComponent>(gunEGID);
                    hudEntityView.ammoComponent.ammo = playerGunComponent.ammo;
                }

                yield return null;
            }
        }

        public UpdateAmmoEngine(IEntityStreamConsumerFactory consumerFactory) { _consumerFactory = consumerFactory; }
        
        readonly IEntityStreamConsumerFactory _consumerFactory;
        IEnumerator                           _listenForEnemyDeath;
    }
}