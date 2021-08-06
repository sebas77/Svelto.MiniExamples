using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Example.Survive.Pickups;
using Svelto.ECS.Example.Survive.ResourceManager;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public class AmmoPickupFactory
    {
        public AmmoPickupFactory(GameObjectFactory gameObjectFactory, IEntityFactory entityFactory)
        {
            _gameobjectFactory = gameObjectFactory;
            _entityFactory     = entityFactory;
        }

        public IEnumerator Build(int ammo, bool startSpawned, string prefab,float x, float y, float z)
        {
            yield return Build(ammo, startSpawned, prefab, new Vector3(x,y,z));
        }

        public IEnumerator Build(int _ammo, bool startSpawned, string prefab, Vector3 spawnLocation)
        {

            var build = _gameobjectFactory.Build("Ammo", false);

            while (build.MoveNext())
                yield return null;

            GameObject ammoGO = build.Current;

            //implementors are ONLY necessary if you need to wrap objects around entity view structs. In the case
            //of Unity, they are needed to wrap Monobehaviours and not used in any other case

            List<IImplementor> implementors = new List<IImplementor>();
            ammoGO.GetComponentsInChildren(implementors);
            var egidHolderImplementor = ammoGO.AddComponent<EntityReferenceHolderImplementor>();
            implementors.Add(egidHolderImplementor);

            ExclusiveBuildGroup buildGroup;
            if (startSpawned)
                buildGroup = AmmoPickups.BuildGroup;
            else
                buildGroup = RecyclableAmmoPickups.BuildGroup;

            //using the GameObject GetInstanceID() as entityID would help to directly use the result of Unity functions
            //to index the entity in the Svelto database. However I want in this demo how to not rely on it.
            var egid = new EGID(_ammoCreated++, buildGroup);
            var initializer = _entityFactory.BuildEntity<AmmoEntityDescriptor>(egid, implementors);

            egidHolderImplementor.reference = initializer.reference;
            
            initializer.Init(new AmmoPickupComponent 
            {
                ammo = 20 
            });

            var ammoImplementor = ammoGO.AddComponent<AmmoImplementor>();
            ammoImplementor.SetSpawn(startSpawned);

            ammoGO.SetActive(true);
            ammoGO.transform.position = spawnLocation;
        }

        readonly IEntityFactory    _entityFactory;
        readonly GameObjectFactory _gameobjectFactory;
        uint                       _ammoCreated;
    }
}