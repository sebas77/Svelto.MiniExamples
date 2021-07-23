using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemyWaveSpawnerEngine : IQueryingEntitiesEngine, IReactOnSwap<EnemyEntityViewComponent>, IStepEngine
    {
        const int SECONDS_BETWEEN_CHECKS = 1;
        const int SECONDS_BEFORE_NEXT_WAVE = 1;
        const int STARTING_WAVE_ENEMY_COUNT = 12;
        const float DIFFICULTY_GRADIENT = 2.0f; // The increase of difficulty between waves
        const int DIFFICULTY_WAVE_LIMIT = 5;    // The wave the difficulty will no longer increase after this wave

        public EnemyWaveSpawnerEngine(EnemyFactory enemyFactory, IEntityFunctions entityFunctions)
        {
            _entityFunctions      = entityFunctions;
            _enemyFactory         = enemyFactory;
            SetWave(0);
            _numberOfDeadEnemies    = _numberOfEnemiesThisWave;
        }

        public EntitiesDB entitiesDB { private get; set; }
        public void       Ready()    { _intervaledTick = IntervaledTick(); }
        public void       Step()     { _intervaledTick.MoveNext(); }
        public string     name       => nameof(EnemySpawnerEngine);

        
        public void MovedTo(ref EnemyEntityViewComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            //is the enemy dead?
            if (egid.groupID.FoundIn(DeadEnemies.Groups))
            {
                _numberOfDeadEnemies++;
            }
        }
        
        void SetWave(int wave) {
            _wave = wave;
            if (wave <= DIFFICULTY_WAVE_LIMIT)
                _numberOfEnemiesThisWave = STARTING_WAVE_ENEMY_COUNT + (int)((wave - 1) * DIFFICULTY_GRADIENT);
            Debug.Log("Incresing number of enemies to: " + _numberOfEnemiesThisWave + "    (" + wave + ")");
        }


        // Produces a list of enemies to spawn for a given wave.
        // Each element in the list is the index of the enemy in the list of all enemies.
        List<int> calculateEnemiesToSpawn(int numberOfEnemiesThisWave, float[] spawningTimes)
        {
            float largestSpawningTime = 0.0f;
            foreach(float t in spawningTimes)
            {
                if (t > largestSpawningTime)
                {
                    largestSpawningTime = t;
                }
            }
            
            float[] spawningFrequency = new float[spawningTimes.Length];

            int numberOfDifferentEnemies = spawningTimes.Length;
            for (int i = 0; i < numberOfDifferentEnemies; i++)
            {
                spawningFrequency[i] = largestSpawningTime / spawningTimes[i];
                //spawningFrequency[i] = 1 / spawningTimes[i];
            }

            float totalSpawningFrequency = 0.0f;
            foreach(float t in spawningFrequency)
            {
                totalSpawningFrequency += t;
            }

            List<int> enemiesToSpawn = new List<int>(); 

            for (int i = 0; i < numberOfDifferentEnemies; i++)
            {
                float probability = spawningFrequency[i] / totalSpawningFrequency;
                int numberOfEnemiesToSpawn = (int)(probability * numberOfEnemiesThisWave);
                Debug.Log("no_en_spwn: " + numberOfEnemiesToSpawn);
                for (int n = 0; n < numberOfEnemiesToSpawn; n++)
                {
                    enemiesToSpawn.Add(i);
                    //Debug.Log(i);
                }
            }
            
            return enemiesToSpawn;
        }


        IEnumerator IntervaledTick()
        {
            //this is of fundamental importance: Never create implementors as Monobehaviour just to hold 
            //data (especially if read only data). Data should always been retrieved through a service layer
            //regardless the data source.
            //The benefits are numerous, including the fact that changing data source would require
            //only changing the service code. In this simple example I am not using a Service Layer
            //but you can see the point.          
            //Also note that I am loading the data only once per application run, outside the 
            //main loop. You can always exploit this pattern when you know that the data you need
            //to use will never change            
            IEnumerator<JSonEnemySpawnData[]>  enemiestoSpawnJsons  = ReadEnemySpawningDataServiceRequest();
            IEnumerator<JSonEnemyAttackData[]> enemyAttackDataJsons = ReadEnemyAttackDataServiceRequest();

            while (enemiestoSpawnJsons.MoveNext())
                yield return null;
            while (enemyAttackDataJsons.MoveNext())
                yield return null;

            var enemiesSpawnData  = enemiestoSpawnJsons.Current;
            var enemyAttackData = enemyAttackDataJsons.Current;

            var spawningTimes = new float[enemiesSpawnData.Length];

            for (var i = enemiesSpawnData.Length - 1; i >= 0; --i)
                spawningTimes[i] = enemiesSpawnData[i].enemySpawnData.spawnTime;

            while (true)
            {
                //Svelto.Tasks can yield Unity YieldInstructions but this comes with a performance hit
                //so the fastest solution is always to use custom enumerators. To be honest the hit is minimal
                //but it's better to not abuse it.                
                var waitForSecondsEnumerator = new WaitForSecondsEnumerator(SECONDS_BETWEEN_CHECKS);
                while (waitForSecondsEnumerator.MoveNext())
                    yield return null;

                Debug.Log("Number of Dead Enemies: " + _numberOfDeadEnemies);
                Debug.Log("Number of Enemies this Wave: " + _numberOfEnemiesThisWave);
                Debug.Log(_numberOfDeadEnemies >= _numberOfEnemiesThisWave);


                // Check if the wave is over
                if (_numberOfDeadEnemies >= _numberOfEnemiesThisWave)
                {
                    // Wait before starting the next wave
                    waitForSecondsEnumerator = new WaitForSecondsEnumerator(SECONDS_BEFORE_NEXT_WAVE);
                    while (waitForSecondsEnumerator.MoveNext())
                        yield return null;

                    SetWave(++_wave);
                    // Spawn enemies for this wave
                    var enemiesToSpawn = calculateEnemiesToSpawn(_numberOfEnemiesThisWave, spawningTimes); 
                    _numberOfEnemiesThisWave = enemiesToSpawn.Count;
                    _numberOfDeadEnemies = 0;
                    for (int i = 0; i < enemiesToSpawn.Count; i++)
                    {
                        var spawnData = enemiesSpawnData[enemiesToSpawn[i]];

                        var EnemyAttackComponent = new EnemyAttackComponent
                        {
                            attackDamage      = enemyAttackData[enemiesToSpawn[i]].enemyAttackData.attackDamage
                          , timeBetweenAttack = enemyAttackData[enemiesToSpawn[i]].enemyAttackData.timeBetweenAttacks
                        };

                        //have we got a compatible entity previously disabled and can it be reused?
                        var fromGroupId = ECSGroups.EnemiesToRecycleGroups + (uint) spawnData.enemySpawnData.targetType;

                        if (entitiesDB.HasAny<EnemyEntityViewComponent>(fromGroupId))
                        {
                            ReuseEnemy(fromGroupId, spawnData);
                            yield return null;
                        }
                        else
                        {
                            var build = _enemyFactory.Build(spawnData.enemySpawnData, EnemyAttackComponent);
                            while (build.MoveNext())
                                yield return null;
                        }
                    }
                }
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
        void ReuseEnemy(ExclusiveGroupStruct fromGroupId, JSonEnemySpawnData spawnData)
        {
            Svelto.Console.LogDebug("reuse enemy " + spawnData.enemySpawnData.enemyPrefab);

            var (healths, enemyViews, count) =
                entitiesDB.QueryEntities<HealthComponent, EnemyEntityViewComponent>(fromGroupId);

            if (count > 0)
            {
                healths[0].currentHealth = 100;

                var spawnInfo = spawnData.enemySpawnData.spawnPoint;

                enemyViews[0].transformComponent.position           = spawnInfo;
                enemyViews[0].movementComponent.navMeshEnabled      = true;
                enemyViews[0].movementComponent.setCapsuleAsTrigger = false;
                enemyViews[0].layerComponent.layer                  = GAME_LAYERS.ENEMY_LAYER;
                enemyViews[0].animationComponent.reset              = true;

                _entityFunctions.SwapEntityGroup<EnemyEntityDescriptor>(enemyViews[0].ID, AliveEnemies.BuildGroup);
            }
        }

        static IEnumerator<JSonEnemySpawnData[]> ReadEnemySpawningDataServiceRequest()
        {
            var json = Addressables.LoadAssetAsync<TextAsset>("EnemySpawningData");

            while (json.IsDone == false)
                yield return null;

            var enemiestoSpawn = JsonHelper.getJsonArray<JSonEnemySpawnData>(json.Result.text);

            yield return enemiestoSpawn;
        }

        static IEnumerator<JSonEnemyAttackData[]> ReadEnemyAttackDataServiceRequest()
        {
            var json = Addressables.LoadAssetAsync<TextAsset>("EnemyAttackData");

            while (json.IsDone == false)
                yield return null;

            var enemiesAttackData = JsonHelper.getJsonArray<JSonEnemyAttackData>(json.Result.text);

            yield return enemiesAttackData;
        }

        readonly EnemyFactory     _enemyFactory;
        readonly IEntityFunctions _entityFunctions;

        int         _numberOfDeadEnemies;
        int         _numberOfEnemiesThisWave;
        int         _wave;
        IEnumerator _intervaledTick;
    }
}
