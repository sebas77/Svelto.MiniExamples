using System.Collections;
using Svelto.Tasks.Enumerators;
using System.IO;
using Svelto.Common;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemySpawnerEngine : IQueryingEntitiesEngine, IStep
    {
        public EnemySpawnerEngine(IEnemyFactory enemyFactory, IEntityFunctions entityFunctions)
        {
            _entityFunctions = entityFunctions;
            _enemyFactory = enemyFactory;
            _numberOfEnemyToSpawn = 15;
        }
        
        public IEntitiesDB entitiesDB { private get; set; }

        public void Ready()
        {
            IntervaledTick().Run();
        }

        IEnumerator IntervaledTick()
        {
//this is of fundamental importance: Never create implementors as Monobehaviour just to hold 
//data (especially if read only data). Data should always been retrieved through a service layer
//regardless the data source.
//The benefit are numerous, including the fact that changing data source would require
//only changing the service code. In this simple example I am not using a Service Layer
//but you can see the point.          
//Also note that I am loading the data only once per application run, outside the 
//main loop. You can always exploit this trick when you now that the data you need
//to use will never change            
            var enemiestoSpawn = ReadEnemySpawningDataServiceRequest();
            var enemyAttackData = ReadEnemyAttackDataServiceRequest();
            
            float[] spawningTimes = new float[enemiestoSpawn.Length];

            for (int i = enemiestoSpawn.Length - 1; i >= 0 && _numberOfEnemyToSpawn > 0; --i)
                spawningTimes[i] = enemiestoSpawn[i].enemySpawnData.spawnTime;

            _enemyFactory.Preallocate();
            
            while (true)
            {
//Svelto.Tasks can yield Unity YieldInstructions but this comes with a performance hit
//so the fastest solution is always to use custom enumerators. To be honest the hit is minimal
//but it's better to not abuse it.                
                yield return _waitForSecondsEnumerator;

                using (var profiler = new PlatformProfiler("EnemySpawning"))
                {
                    //cycle around the enemies to spawn and check if it can be spawned
                    for (int i = enemiestoSpawn.Length - 1; i >= 0 && _numberOfEnemyToSpawn > 0; --i)
                    {
                        if (spawningTimes[i] <= 0.0f)
                        {
                            var spawnData = enemiestoSpawn[i];

                            //In this example every kind of enemy generates the same list of EntityViews
                            //therefore I always use the same EntityDescriptor. However if the 
                            //different enemies had to create different EntityViews for different
                            //engines, this would have been a good example where EntityDescriptorHolder
                            //could have been used to exploit the the kind of polymorphism explained
                            //in my articles.
                            EnemyAttackStruct enemyAttackStruct = new EnemyAttackStruct
                            {
                                attackDamage      = enemyAttackData[i].enemyAttackData.attackDamage,
                                timeBetweenAttack = enemyAttackData[i].enemyAttackData.timeBetweenAttacks
                            };

                            //has got a compatible entity previously disabled and can be reused?
                            //Note, pooling make sense only for Entities that use implementors.
                            //A pure struct based entity doesn't need pooling because it never allocates.
                            //to simplify the logic, we use a recycle group for each entity type
                            var fromGroupId = (int) ECSGroups.EnemiesToRecycleGroups +
                                              (int) spawnData.enemySpawnData.targetType;

                            if (entitiesDB.HasAny<EnemyEntityViewStruct>(fromGroupId))
                            {
                                using (profiler.Sample("Recycling"))
                                {
                                    ReuseEnemy(fromGroupId, spawnData);
                                }
                            }
                            else
                            {
                                using (profiler.Sample("Building"))
                                {
                                    _enemyFactory.Build(spawnData.enemySpawnData, ref enemyAttackStruct);
                                }
                            }

                            spawningTimes[i] = spawnData.enemySpawnData.spawnTime;
                            _numberOfEnemyToSpawn--;
                        }

                        spawningTimes[i] -= SECONDS_BETWEEN_SPAWNS;
                    }
                }
            }
        }

        /// <summary>
        /// Reset all the component values when an Enemy is ready to be recycled.
        /// it's important to not forget to reset all the states.
        /// note that the only reason why we pool it the entities here is to reuse the implementors,
        /// pure entity structs entities do not need pool and can be just recreated
        /// </summary>
        /// <param name="spawnData"></param>
        /// <returns></returns>

        void ReuseEnemy(int fromGroupId, JSonEnemySpawnData spawnData)
        {
            int count;
            var healths = entitiesDB.QueryEntities<HealthEntityStruct>(fromGroupId, out count);
           
            if (count > 0)
            {
                var enemystructs = entitiesDB.QueryEntities<EnemyEntityViewStruct>(fromGroupId, out count);
                healths[0].currentHealth = 100;
                healths[0].dead          = false;

                var spawnInfo = spawnData.enemySpawnData.spawnPoint;

                enemystructs[0].transformComponent.position = spawnInfo;
                enemystructs[0].movementComponent.navMeshEnabled      = true;
                enemystructs[0].movementComponent.setCapsuleAsTrigger = false;
                enemystructs[0].layerComponent.layer                  = GAME_LAYERS.ENEMY_LAYER;
                enemystructs[0].animationComponent.reset = true;
                
                _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(enemystructs[0].ID, ECSGroups.ActiveEnemies);
            }
        }

        static JSonEnemySpawnData[] ReadEnemySpawningDataServiceRequest()
        {
            string json = File.ReadAllText("EnemySpawningData.json");
            
            JSonEnemySpawnData[] enemiestoSpawn = JsonHelper.getJsonArray<JSonEnemySpawnData>(json);
            
            return enemiestoSpawn;
        }
        
        static JSonEnemyAttackData[] ReadEnemyAttackDataServiceRequest()
        {
            string json = File.ReadAllText("EnemyAttackData.json");
            
            JSonEnemyAttackData[] enemiestoSpawn = JsonHelper.getJsonArray<JSonEnemyAttackData>(json);
            
            return enemiestoSpawn;
        }
        
        public void Step(EGID id)
        {
            _numberOfEnemyToSpawn++;
        }

        readonly WaitForSecondsEnumerator  _waitForSecondsEnumerator = new WaitForSecondsEnumerator(SECONDS_BETWEEN_SPAWNS);
        
        readonly IEnemyFactory _enemyFactory;
        readonly IEntityFunctions _entityFunctions;

        int     _numberOfEnemyToSpawn;
        
        const int SECONDS_BETWEEN_SPAWNS = 1;
    }
}
