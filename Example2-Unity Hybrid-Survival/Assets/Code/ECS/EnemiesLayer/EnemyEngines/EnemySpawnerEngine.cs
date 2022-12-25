using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public class EnemySpawnerEngine: IQueryingEntitiesEngine, IReactOnSwap<EnemyEntityViewComponent>, IStepEngine
    {
        const int SECONDS_BETWEEN_SPAWNS = 1;
        const int NUMBER_OF_ENEMIES_TO_SPAWN = 1;

        public EnemySpawnerEngine(EnemyFactory enemyFactory)
        {
            _enemyFactory = enemyFactory;
            _numberOfEnemyToSpawn = NUMBER_OF_ENEMIES_TO_SPAWN;
        }

        public EntitiesDB entitiesDB { private get; set; }
        public void Ready() { _intervaledTick = IntervaledTick(); }
        public void Step() { _intervaledTick.MoveNext(); }
        public string name => nameof(EnemySpawnerEngine);

        public void MovedTo(ref EnemyEntityViewComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            //is the enemy dead?
            if (egid.groupID.FoundIn(DeadEnemies.Groups))
            {
                _numberOfEnemyToSpawn++;
            }
        }

        IEnumerator IntervaledTick()
        {
            var enemiestoSpawnTask = PreallocationTask();
            while (enemiestoSpawnTask.IsCompleted == false)
                yield return null;

            var (enemiestoSpawn, enemyAttackData, spawningTimes) = enemiestoSpawnTask.Result;

            while (true)
            {
                //cycle around the enemies to spawn and check if it can be spawned
                for (var i = enemiestoSpawn.Length - 1; i >= 0 && _numberOfEnemyToSpawn > 0; --i)
                {
                    if (spawningTimes[i] <= 0.0f)
                    {
                        var spawnData = enemiestoSpawn[i];

                        //In this example every kind of enemy generates the same list of components
                        //therefore I always use the same EntityDescriptor.
                        var EnemyAttackComponent = new EnemyAttackComponent
                        {
                                attackDamage = enemyAttackData[i].enemyAttackData.attackDamage,
                                timeBetweenAttack = enemyAttackData[i].enemyAttackData.timeBetweenAttacks
                        };

                        var task = _enemyFactory.Fetch(spawnData.enemySpawnData, EnemyAttackComponent);
                        while (task.GetAwaiter().IsCompleted == false)
                            yield return null;

                        spawningTimes[i] = spawnData.enemySpawnData.spawnTime;
                        _numberOfEnemyToSpawn--;
                    }

                    spawningTimes[i] -= SECONDS_BETWEEN_SPAWNS;
                }

                var waitForSecondsEnumerator = new WaitForSecondsEnumerator(SECONDS_BETWEEN_SPAWNS);
                while (waitForSecondsEnumerator.MoveNext())
                    yield return null;
            }

            async Task<(JSonEnemySpawnData[] enemiestoSpawn, JSonEnemyAttackData[] enemyAttackData, float[]
                spawningTimes)> PreallocationTask()
            {
                //Data should always been retrieved through a service layer regardless the data source.
                //The benefits are numerous, including the fact that changing data source would require
                //only changing the service code. In this simple example I am not using a Service Layer
                //but you can see the point.          
                //Also note that I am loading the data only once per application run, outside the 
                //main loop. You can always exploit this pattern when you know that the data you need
                //to use will never change            
                var enemiestoSpawn = await ReadEnemySpawningDataServiceRequest();
                var enemyAttackData = await ReadEnemyAttackDataServiceRequest();

                var spawningTimes = new float[enemiestoSpawn.Length];

                //prebuild gameobjects to avoid spikes. For each enemy type
                for (var i = enemiestoSpawn.Length - 1; i >= 0; --i)
                {
                    var spawnData = enemiestoSpawn[i];

                    //preallocate the max number of enemies
                    await _enemyFactory.Preallocate(spawnData.enemySpawnData, NUMBER_OF_ENEMIES_TO_SPAWN);

                    spawningTimes[i] = enemiestoSpawn[i].enemySpawnData.spawnTime;
                }

                return (enemiestoSpawn, enemyAttackData, spawningTimes);
            }
        }

        static async Task<JSonEnemySpawnData[]> ReadEnemySpawningDataServiceRequest()
        {
            var json = await Addressables.LoadAssetAsync<TextAsset>("EnemySpawningData").Task;

            JSonEnemySpawnData[] enemiestoSpawn = JsonHelper.getJsonArray<JSonEnemySpawnData>(json.text);

            return enemiestoSpawn;
        }

        static async Task<JSonEnemyAttackData[]> ReadEnemyAttackDataServiceRequest()
        {
            var json = await Addressables.LoadAssetAsync<TextAsset>("EnemyAttackData").Task;

            var enemiesAttackData = JsonHelper.getJsonArray<JSonEnemyAttackData>(json.text);

            return enemiesAttackData;
        }

        readonly EnemyFactory _enemyFactory;

        int _numberOfEnemyToSpawn;
        IEnumerator _intervaledTick;
    }
}