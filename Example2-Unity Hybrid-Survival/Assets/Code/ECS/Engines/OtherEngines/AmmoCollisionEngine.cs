using System;
using Svelto.Common;
using UnityEngine;

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
            Svelto.Console.LogDebug(sender + "");
            //entitiesDB.QueryEntity<>
        }

        public string name => nameof(AmmoCollisionEngine);

        readonly Action<EGID, AmmoCollisionData> _onCollidedWithTarget;
    }
}

