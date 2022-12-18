using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.Enemies;
using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public class EnemyFactory
    {
        public EnemyFactory(IEntityFactory entityFactory, GameObjectResourceManager gameObjectResourceManager)
        {
            _entityFactory = entityFactory;
            _gameObjectResourceManager = gameObjectResourceManager;
        }
        
        public async Task Preallocate(EnemySpawnData enemySpawnData, int numberOfEnemiesToSpawn)
        {
            await _gameObjectResourceManager.Preallocate(enemySpawnData.enemyPrefab, (int)enemySpawnData.targetType, numberOfEnemiesToSpawn);
        }

        public async Task Fetch(EnemySpawnData enemySpawnData, EnemyAttackComponent EnemyAttackComponent)
        {
            void InitEntity(List<IImplementor> list, EntityReferenceHolder entityReferenceHolder, GameObject enemyGo, ValueIndex valueIndex)
            {
                //using the GameObject GetInstanceID() as entityID would help to directly use the result of Unity functions
                //to index the entity in the Svelto database. However I want in this demo how to not rely on it.
                EntityInitializer initializer =
                        _entityFactory.BuildEntity<EnemyEntityDescriptor>(
                            new EGID(_enemiesCreated++, AliveEnemies.BuildGroup)
                          , list);

                entityReferenceHolder.reference = initializer.reference;
                //Initialize the pure EntityStructs. This should be the preferred pattern, there is much less boiler plate
                //too
                initializer.Init(EnemyAttackComponent);
                initializer.Init(
                    new HealthComponent
                    {
                            currentHealth = 100
                    });
                initializer.Init(
                    new ScoreValueComponent
                    {
                            scoreValue = (int)(enemySpawnData.targetType + 1) * 10
                    });
                initializer.Init(
                    new EnemyComponent
                    {
                            enemyType = enemySpawnData.targetType
                    });

                var transform = enemyGo.transform;
                var spawnPoint = enemySpawnData.spawnPoint;

                enemyGo.SetActive(true);
                transform.position = spawnPoint;

                initializer.Init(
                    new GameObjectEntityComponent
                    {
                            resourceIndex = valueIndex
                    });
            }

            var build = await _gameObjectResourceManager.Reuse(enemySpawnData.enemyPrefab, (int)enemySpawnData.targetType);

            ValueIndex gameObjectIndex = build;
            var enemyGO = _gameObjectResourceManager[gameObjectIndex];
            
            //implementors are ONLY necessary if you need to wrap objects around entity view structs. In the case
            //of Unity, they are needed to wrap Monobehaviours and not used in any other case

            List<IImplementor> implementors = new List<IImplementor>();
            enemyGO.GetComponentsInChildren(implementors);
            var egidHolderImplementor = enemyGO.AddComponent<EntityReferenceHolder>();

            InitEntity(implementors, egidHolderImplementor, enemyGO, gameObjectIndex);
        }

        readonly IEntityFactory _entityFactory;
        readonly GameObjectResourceManager _gameObjectResourceManager;
        uint _enemiesCreated;
    }
}