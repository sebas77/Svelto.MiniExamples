using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Svelto.ECS.Example.Survive.ResourceManager;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.Survive.Weapons
{
    public class AmmoSpawnerEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public AmmoSpawnerEngine(GameObjectFactory gameobjectFactory, IEntityFactory entityFactory)
        {
            _gameobjectFactory = gameobjectFactory;
            _entityFactory = entityFactory;
        }

        public EntitiesDB entitiesDB { private get; set; }

        public void Ready() { 
            _spawnAmmo = SpawnAmmo();
            _rotateAmmo = RotateAmmo();
        }

        public void Step() { 
            _spawnAmmo.MoveNext();
            _rotateAmmo.MoveNext();
        }
        public string name => nameof(AmmoSpawnerEngine);

        IEnumerator SpawnAmmo()
        {
            IEnumerator<GameObject> ammoLoading = _gameobjectFactory.Build("AmmoCrate");

            while(ammoLoading.MoveNext()) yield return null;

            GameObject ammoObject = ammoLoading.Current;

            //Implementors
            List<IImplementor> implementors = new List<IImplementor>();
            ammoObject.GetComponentsInChildren(true, implementors);

            //EGID
            EntityReferenceHolderImplementor egidHoldImplementor = ammoObject.AddComponent<EntityReferenceHolderImplementor>();
            implementors.Add(egidHoldImplementor);

            //initialise
            EntityInitializer ammoInitializer = _entityFactory.BuildEntity<AmmoEntityDescriptor>(0, AmmoTag.BuildGroup, implementors);
            ammoInitializer.Init(new AmmoValueComponent
            {
                ammoValue = 20
            });
            //ammoObject.transform.position = new Vector3(10, 0.75f, 10);

            egidHoldImplementor.reference = ammoInitializer.reference;
        }

        IEnumerator RotateAmmo()
        {
            void Rotate()
            {
                foreach (var ((ammo, ammoCount), _) in entitiesDB.QueryEntities<AmmoEntityViewComponent>(
                        AmmoTag.Groups))
                {
                    //Svelto.Console.LogDebug(ammo[0].ID + "");
                    ammo[0].ammoComponent.rotation *= Quaternion.Euler(Vector3.up * 1);
                }

            }

            while (true)
            {
                Rotate();

                yield return null;
            }
        }

        readonly IEntityFactory      _entityFactory;
        readonly GameObjectFactory   _gameobjectFactory;
        IEnumerator                  _spawnAmmo;
        IEnumerator                  _rotateAmmo;
    }
}