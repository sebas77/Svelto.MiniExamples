using Svelto.Common;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.ECS.Example.Survive.Characters.Enemies;
using Svelto.ECS.Example.Survive.HUD;
using Svelto.Factories;
using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    class EnemyFactory : IEnemyFactory
    {
        public EnemyFactory(IGameObjectFactory gameObjectFactory,
                            IEntityFactory entityFactory)
        {
            _gameobjectFactory = gameObjectFactory;
            _entityFactory = entityFactory;
        }
        
        public void Build(EnemySpawnData enemySpawnData, ref EnemyAttackStruct enemyAttackstruct)
        {
            // Find a random index between zero and one less than the number of spawn points.
            // Create an instance of the enemy prefab at the randomly selected spawn point position and rotation.
            using (var profiler = new PlatformProfiler("BuildEnemy"))
            {
                GameObject go;
                using (profiler.Sample("Build GameObject"))
                {
                    go = _gameobjectFactory.Build(enemySpawnData.enemyPrefab);
                }

                IImplementor[] implementors;
                using (profiler.Sample("Get Components In Children"))
                {
                    implementors = go.GetComponentsInChildren<IImplementor>();
                }
                //using the GameObject GetInstanceID() will help to directly use the result of Unity functions
                //to index the entity in the Svelto database

                EntityStructInitializer initializer;
                using (profiler.Sample("BuildEntity"))
                {
                    initializer = _entityFactory
                       .BuildEntity<EnemyEntityDescriptor>(new EGID(go.GetInstanceID(), ECSGroups.ActiveEnemies), implementors);
                }
                
                initializer.Init(enemyAttackstruct);
                initializer.Init(new HealthEntityStruct {currentHealth  = 100});
                initializer.Init(new ScoreValueEntityStruct {scoreValue = (int) (enemySpawnData.targetType + 1) * 10});
                initializer.Init(new EnemyEntityStruct {enemyType       = enemySpawnData.targetType});
                initializer.Init(new EnemySinkStruct
                                     {sinkAnimSpeed = 2.5f}); //being lazy, should come from the json too

                var transform = go.transform;
                var spawnInfo = enemySpawnData.spawnPoint;

                transform.position = spawnInfo;
            }
        }

        public void Preallocate()
        {
            _entityFactory.PreallocateEntitySpace<EnemyEntityDescriptor>(ECSGroups.ActiveEnemies, 10);
        }

        readonly IGameObjectFactory _gameobjectFactory;
        readonly IEntityFactory     _entityFactory;
    }
}