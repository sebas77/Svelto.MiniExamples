using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Svelto.ECS.Example.Survive.Pickups
{
    public class AmmoSpawnerEngine : IQueryingEntitiesEngine, IReactOnSwap<AmmoPickupEntityViewComponent>, IStepEngine
    {
        const int SECONDS_BETWEEN_SPAWNS = 1;
        const int NUMBER_OF_ENEMIES_TO_SPAWN   = 12;

        public AmmoSpawnerEngine(AmmoPickupFactory ammoFactory, IEntityFunctions entityFunctions)
        {
            _entityFunctions      = entityFunctions;
            _ammoFactory         = ammoFactory;
            _numberOfEnemyToSpawn = NUMBER_OF_ENEMIES_TO_SPAWN;
        }

        public EntitiesDB entitiesDB { private get; set; }
        public void       Ready()    { _intervaledTick = IntervaledTick(); }
        public void       Step()     { _intervaledTick.MoveNext(); }
        public string     name       => nameof(AmmoSpawnerEngine);

        public void MovedTo(ref AmmoPickupEntityViewComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            //is the ammo destroyed?
            if (egid.groupID.FoundIn(RecyclableAmmoPickups.Groups))
            {
                _numberOfEnemyToSpawn++;
            }
        }

        IEnumerator IntervaledTick()
        {
            IEnumerator<JsonAmmoSpawnData> ammoSpawnDataJsons = AmmoSpawnDataService.Load();

            while (ammoSpawnDataJsons.MoveNext())
                yield return null;

            var ammoSpawnData = ammoSpawnDataJsons.Current;

            // Initialize every ammo pickup entity and set them to the 
            // DestroyedAmmoPickup group
            for (var i = ammoSpawnData.spawnLocations.Length - 1; i >= 0; --i)
            {
                var build = _ammoFactory.Build(
                    ammoSpawnData.ammoPerPickup,
                    false,
                    ammoSpawnData.ammoPrefab,
                    new Vector3(
                        ammoSpawnData.spawnLocations[i].x,
                        ammoSpawnData.spawnLocations[i].y,
                        ammoSpawnData.spawnLocations[i].z
                    )
                );
                while (build.MoveNext())
                    yield return null;
                
            }

            var secondsBetweenSpawn = ammoSpawnData.secondsBetweenSpawns;

            while (true)
            {
                //Svelto.Tasks can yield Unity YieldInstructions but this comes with a performance hit
                //so the fastest solution is always to use custom enumerators. To be honest the hit is minimal
                //but it's better to not abuse it.                
                var waitForSecondsEnumerator = new WaitForSecondsEnumerator(secondsBetweenSpawn);
                while (waitForSecondsEnumerator.MoveNext())
                    yield return null;

                // find a free spawnable entity and re-use it
                if (entitiesDB.HasAny<AmmoPickupEntityViewComponent>(RecyclableAmmoPickups.BuildGroup))
                    ReuseAmmo();
            }
        }

        /// <summary>
        ///     Reset all the component values when an Enemy is ready to be recycled.
        ///     it's important to not forget to reset all the states.
        ///     note that the only reason why we pool it the entities here is to reuse the implementors,
        ///     pure entity structs entities do not need pool and can be just recreated
        /// </summary>
        /// <param name="spawnData"></param>
        /// <returns></returns>
        void ReuseAmmo()
        {
            Svelto.Console.LogDebug("reusing ammo");

            var (ammo, count)=
                entitiesDB.QueryEntities<AmmoPickupEntityViewComponent>(RecyclableAmmoPickups.BuildGroup);

            if (count > 0)
            {
                ammo[0].spawnedComponent.spawned = true;
                _entityFunctions.SwapEntityGroup<AmmoEntityDescriptor>(ammo[0].ID, AmmoPickups.BuildGroup);
            }
        }

        readonly AmmoPickupFactory _ammoFactory;
        readonly IEntityFunctions  _entityFunctions;

        int         _numberOfEnemyToSpawn;
        IEnumerator _intervaledTick;
    }

}