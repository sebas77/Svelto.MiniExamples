using System.Collections.Generic;
using System.Threading.Tasks;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.Enemies;
using Svelto.ECS.Example.Survive.OOPLayer;
using Svelto.ECS.Example.Survive.Transformable;
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

        public async Task Fetch(EnemySpawnData enemySpawnData, JSonEnemyAttackData enemyAttackComponent)
        {
            void InitEntity(List<IImplementor> list, EntityReferenceHolder entityReferenceHolder, GameObject enemyGo, ValueIndex valueIndex)
            {
                //using the GameObject GetInstanceID() as entityID would help to directly use the result of Unity functions
                //to index the entity in the Svelto database. However I want in this demo how to not rely on it.
                EntityInitializer initializer =
                        _entityFactory.BuildEntity<EnemyEntityDescriptor>(
                            new EGID(_enemiesCreated++, EnemiesGroup.BuildGroup), list);

                entityReferenceHolder.reference = initializer.reference.ToLong();
                //Initialize the pure EntityStructs. This should be the preferred pattern, there is much less boiler plate
                //too
                //In this example every kind of enemy generates the same list of components
                //therefore I always use the same EntityDescriptor.
                initializer.Init(new EnemyAttackComponent
                {
                        attackDamage = enemyAttackComponent.enemyAttackData.attackDamage,
                        timeBetweenAttack = enemyAttackComponent.enemyAttackData.timeBetweenAttacks
                });
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
                initializer.Init(
                    new GameObjectEntityComponent
                    {
                            resourceIndex = valueIndex,
                            layer = GAME_LAYERS.ENEMY_LAYER 
                    });
                initializer.Init(
                    new PositionComponent()
                    {
                            position = enemySpawnData.spawnPoint
                    });

                enemyGo.SetActive(true);
                var implt = enemyGo.GetComponent<EnemyMovementImplementor>();
                implt.navMeshEnabled = true;
                implt.setCapsuleAsTrigger = false;
            }

            var build = await _gameObjectResourceManager.Reuse(enemySpawnData.enemyPrefab, (int)enemySpawnData.targetType);

            ValueIndex gameObjectIndex = build;
            var enemyGO = _gameObjectResourceManager[gameObjectIndex];
            
            //implementors are ONLY necessary if you need to wrap objects around entity view structs. In the case
            //of Unity, they are needed to wrap Monobehaviours and not used in any other case

            List<IImplementor> implementors = new List<IImplementor>();
            enemyGO.GetComponentsInChildren(implementors);
            var egidHolderImplementor = enemyGO.GetComponent<EntityReferenceHolder>();

            InitEntity(implementors, egidHolderImplementor, enemyGO, gameObjectIndex);
        }

        readonly IEntityFactory _entityFactory;
        readonly GameObjectResourceManager _gameObjectResourceManager;
        uint _enemiesCreated;
    }
}