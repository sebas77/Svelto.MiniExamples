using System.Collections;
using System.Collections.Generic;
using Svelto.ECS.Example.Survive.Wave;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemyWaveSpawnerEngine : IQueryingEntitiesEngine, IReactOnSwap<EnemyEntityViewComponent>, IStepEngine
    {
        const int SECONDS_BETWEEN_CHECKS = 1;
        const int SECONDS_BEFORE_NEXT_WAVE = 1;
        const int STARTING_WAVE_ENEMY_COUNT = 12;
        const float DIFFICULTY_GRADIENT = 5.0f; // The increase of difficulty between waves
        const int DIFFICULTY_WAVE_LIMIT = 10;    // The wave the difficulty will no longer increase after this wave

        public EnemyWaveSpawnerEngine(EnemyFactory enemyFactory, IEntityFunctions entityFunctions)
        {
            _entityFunctions      = entityFunctions;
            _enemyFactory         = enemyFactory;
            _wave                 = 0;
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
                ref WaveComponent waveEntity = ref entitiesDB.QueryUniqueEntity<WaveComponent>(ECSGroups.Waves);
                waveEntity.enemiesLeft--;
                entitiesDB.PublishEntityChange<WaveComponent>(ECSGroups.WaveState);
            }
        }
        
        void SetWave(int wave) {
            _wave = wave;
            ref WaveComponent waveEntity = ref entitiesDB.QueryUniqueEntity<WaveComponent>(ECSGroups.Waves);
            waveEntity.wave = wave;
            entitiesDB.PublishEntityChange<WaveComponent>(ECSGroups.WaveState);
            if (wave <= DIFFICULTY_WAVE_LIMIT)
                _numberOfEnemiesThisWave = STARTING_WAVE_ENEMY_COUNT + (int)((wave - 1) * DIFFICULTY_GRADIENT);
        }


        // Produces a list of enemies to spawn for a given wave.
        // Each element in the list is the index of the enemy in the list of all enemies.
        List<int> calculateEnemiesToSpawn(int numberOfEnemiesThisWave, float[] spawningTimes)
        {
            int numberOfDifferentEnemies = spawningTimes.Length;

            // Obtain largest spawning time
            float largestSpawningTime = 0.0f;
            foreach(float t in spawningTimes)
            {
                if (t > largestSpawningTime)
                {
                    largestSpawningTime = t;
                }
            }
            
            // Create list of spawning frequencies
            float[] spawningFrequency = new float[spawningTimes.Length];
            for (int i = 0; i < numberOfDifferentEnemies; i++)
            {
                spawningFrequency[i] = 1 / spawningTimes[i];
            }

            // Obtain total spawning frequency
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
                for (int n = 0; n < numberOfEnemiesToSpawn; n++)
                {
                    enemiesToSpawn.Add(i);
                }
            }
            
            return enemiesToSpawn;
        }


        IEnumerator IntervaledTick()
        {
            IEnumerator<JSonEnemySpawnData[]>  enemiestoSpawnJsons  = ReadEnemySpawningDataServiceRequest();
            IEnumerator<JSonEnemyAttackData[]> enemyAttackDataJsons = ReadEnemyAttackDataServiceRequest();

            while (enemiestoSpawnJsons.MoveNext())
                yield return null;
            while (enemyAttackDataJsons.MoveNext())
                yield return null;

            var enemiesSpawnData  = enemiestoSpawnJsons.Current;
            var enemyAttackData = enemyAttackDataJsons.Current;

            var spawningTimes = new float[enemiesSpawnData.Length];

            while (entitiesDB.HasAny<WaveComponent>(ECSGroups.Waves) == false)
            {
                yield return null;
            }

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


                // Check if the wave is over
                if (_numberOfDeadEnemies >= _numberOfEnemiesThisWave)
                {
                    // Wait before starting the next wave
                    waitForSecondsEnumerator = new WaitForSecondsEnumerator(SECONDS_BEFORE_NEXT_WAVE);
                    while (waitForSecondsEnumerator.MoveNext())
                        yield return null;

                    SetWave(++_wave);
                    var waveEntity = entitiesDB.QueryUniqueEntity<WaveComponent>(ECSGroups.Waves);

                    // Spawn enemies for this wave
                    var enemiesToSpawn = calculateEnemiesToSpawn(_numberOfEnemiesThisWave, spawningTimes); 
                    _numberOfEnemiesThisWave = enemiesToSpawn.Count;
                    SetWaveEnemiesLeft(_numberOfEnemiesThisWave);
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
        /// Set enemiesLeft field in WaveComponent and publishes the change
        /// </summary>
        void SetWaveEnemiesLeft(int enemiesLeft)
        {
            ref WaveComponent waveEntity = ref entitiesDB.QueryUniqueEntity<WaveComponent>(ECSGroups.Waves);
            waveEntity.enemiesLeft = enemiesLeft;
            entitiesDB.PublishEntityChange<WaveComponent>(ECSGroups.WaveState);
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
