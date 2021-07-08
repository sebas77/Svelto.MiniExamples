using System;
using Svelto.Common;
using UnityEngine;
using Svelto.ECS.Example.Survive.Player.Gun;

namespace Svelto.ECS.Example.Survive.Weapons
{
    public class AmmoCollisionEngine : IReactOnAddAndRemove<AmmoEntityViewComponent>, IQueryingEntitiesEngine, IStepEngine
    {
        public AmmoCollisionEngine() {
            _onCollidedWithTarget = OnCollidedWithTarget;
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() {  }

        public void Add(ref AmmoEntityViewComponent ammoEntityViewComponent, EGID egid)
        {
            ammoEntityViewComponent.triggerComponent.hitChange = new DispatchOnChange<AmmoCollisionData>(egid, _onCollidedWithTarget);
        }

        public void Remove(ref AmmoEntityViewComponent ammoEntityViewComponent, EGID egid)
        {

        }

        public void Step()
        {

        }

        void OnCollidedWithTarget(EGID sender, AmmoCollisionData ammoCollisionData)
        {
            var ammoCollisionComponent = entitiesDB.QueryEntity<AmmoCollisionComponent>(sender);
            ammoCollisionComponent.entityInRange = ammoCollisionData;
            ammoCollisionComponent.originEGID = sender;

            if (ammoCollisionData.collides && ammoCollisionData.otherEntityID.ToEGID(entitiesDB, out var otherEntityID))
            {
                var weaponEGID = entitiesDB.QueryEntity<Player.PlayerWeaponComponent>(otherEntityID).weapon.ToEGID(entitiesDB);
                ref var ammoComponent = ref entitiesDB.QueryEntity<AmmoValueComponent>(weaponEGID);
                ref var placedAmmoComponent = ref entitiesDB.QueryEntity<AmmoValueComponent>(ammoCollisionComponent.originEGID);
                ammoComponent.ammoValue += placedAmmoComponent.ammoValue;
                var ammoEntityViewComponent = entitiesDB.QueryEntity<AmmoEntityViewComponent>(ammoCollisionComponent.originEGID);
                ammoEntityViewComponent.ammoComponent.position = new Vector3(UnityEngine.Random.Range(-10f, 10f), 1, UnityEngine.Random.Range(-10f, 10f));
                entitiesDB.PublishEntityChange<AmmoValueComponent>(weaponEGID);
            }
        }

        public string name => nameof(AmmoCollisionEngine);

        readonly Action<EGID, AmmoCollisionData> _onCollidedWithTarget;
    }
}

