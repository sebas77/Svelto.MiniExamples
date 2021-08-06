using System;
using Svelto.ECS.Example.Survive.Player.Gun;
using Svelto.ECS.Example.Survive.Player;

namespace Svelto.ECS.Example.Survive.Pickups
{
    public class AmmoPickupEngine : IReactOnAddAndRemove<AmmoPickupEntityViewComponent>
                                   , IReactOnSwap<AmmoPickupEntityViewComponent>, IQueryingEntitiesEngine
    {
        public AmmoPickupEngine(IEntityFunctions entityFunctions)
        {
            _onCollidedWithTarget = OnCollidedWithTarget;
            _entityFunctions      = entityFunctions;
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() {}

        /// <summary>
        ///     The Add and Remove callbacks are enabled by the IReactOnAddAndRemove interface
        ///     They are called when:
        ///     an Entity is built in a group  (no swap case)
        ///     an Entity is removed from a group (no swap case)
        /// </summary>
        /// <param name="entityViewComponent">the up to date entity</param>
        /// <param name="previousGroup">where the entity is coming from</param>
        public void Add(ref AmmoPickupEntityViewComponent entityViewComponent, EGID egid)
        {
            //for each new enemy entity added, we register a new DispatchOnChange
            //DispatchOnChange is a simple solution to let implementors communicate with engine
            //An Implementor can communicate only with an appointed engine and the engine can broadcast the information
            //if necessary.
            //set what callback must be called when the implementor dispatch the value change
            entityViewComponent.targetTriggerComponent.hitChange = new DispatchOnChange<AmmoCollisionData>(egid, _onCollidedWithTarget);
        }

        public void Remove(ref AmmoPickupEntityViewComponent entityViewComponent, EGID egid)
        {
            //for safety we clean up the dispatcher on change in entity removal
            entityViewComponent.targetTriggerComponent.hitChange = null;
        }

        /// <summary>
        ///     MovedTo callbacks are enabled by the IReactOnSwap interface
        ///     They are called on entity swap (when leaving a group and moving to the new one)
        /// </summary>
        /// <param name="entityViewComponent"></param>
        public void MovedTo (ref AmmoPickupEntityViewComponent entityViewComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            entityViewComponent.targetTriggerComponent.hitChange = new DispatchOnChange<AmmoCollisionData>(egid, _onCollidedWithTarget);
            //entityViewComponent.targetTriggerComponent.hitChange.value = value;
            //If the enemy is dead, we pause the collision triggering, it will be renabled if the GO is recycled
            if (egid.groupID.FoundIn(RecyclableAmmoPickups.Groups))
                entityViewComponent.targetTriggerComponent.hitChange.PauseNotify();
            else
                entityViewComponent.targetTriggerComponent.hitChange.ResumeNotify();
        }

        /// <summary>
        /// once the player enters in a trigger, we set the trigger data built inside the implementor and sent
        /// through the DispatchOnChange
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="enemyCollisionData"></param>
        void OnCollidedWithTarget(EGID sender, AmmoCollisionData ammoCollisionData)
        {
            if (ammoCollisionData.otherEntityID.ToEGID(entitiesDB, out var otherEntityID))
            {
                if (otherEntityID.groupID.FoundIn(Player.Player.Groups)) // Check if collision with player
                {
                    // Add to players ammo
                    var weapon = entitiesDB.QueryUniqueEntity<PlayerWeaponComponent>(Player.Player.Groups[0]);
                    var gunEGID            = weapon.weapon.ToEGID(entitiesDB);
                    ref AmmoGunComponent playerGunComponent = ref entitiesDB.QueryEntity<AmmoGunComponent>(gunEGID);
                    var ammo = entitiesDB.QueryEntity<AmmoPickupComponent>(sender).ammo;
                    playerGunComponent.ammo += ammo;
                    entitiesDB.PublishEntityChange<AmmoGunComponent>(gunEGID);


                    // Remove pickup
                    _entityFunctions.SwapEntityGroup<AmmoEntityDescriptor>(sender, RecyclableAmmoPickups.BuildGroup);
                    var AmmoPickupEntityViewComponent = entitiesDB.QueryEntity<AmmoPickupEntityViewComponent>(sender);
                    AmmoPickupEntityViewComponent.spawnedComponent.spawned = false;
                }
            }
        }


        public string name => nameof(AmmoPickupEngine);

        readonly Action<EGID, AmmoCollisionData> _onCollidedWithTarget;
        readonly IEntityFunctions  _entityFunctions;
    }
}