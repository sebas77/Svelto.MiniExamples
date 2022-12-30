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
            await _gameObjectResourceManager.Preallocate(
                enemySpawnData.enemyPrefab, (int)enemySpawnData.targetType, numberOfEnemiesToSpawn);
        }

        public async Task Fetch(EnemySpawnData enemySpawnData, JSonEnemyAttackData enemyAttackComponent)
        {
            void InitEntity(EntityReferenceHolder entityReferenceHolder, GameObject enemyGo, ValueIndex valueIndex)
            {
                //using the GameObject GetInstanceID() as entityID would help to directly use the result of Unity functions
                //to index the entity in the Svelto database. However I want in this demo how to not rely on it.
                EntityInitializer initializer =
                        _entityFactory.BuildEntity<EnemyEntityDescriptor>(
                            new EGID(_enemiesCreated++, EnemyAliveGroup.BuildGroup));

                entityReferenceHolder.reference = initializer.reference.ToULong();
                //Initialize the pure EntityStructs. This should be the preferred pattern, there is much less boiler plate
                //too
                //In this example every kind of enemy generates the same list of components
                //therefore I always use the same EntityDescriptor.
                initializer.Init(
                    new EnemyAttackComponent
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
                initializer.Init(
                    new NavMeshComponent()
                    {
                            navMeshEnabled = true,
                            setCapsuleAsTrigger = false
                    });

                enemyGo.SetActive(true);
            }

            var build = await _gameObjectResourceManager.Reuse(
                enemySpawnData.enemyPrefab, (int)enemySpawnData.targetType);

            ValueIndex gameObjectIndex = build;
            var enemyGO = _gameObjectResourceManager[gameObjectIndex];

            var referencHolder = enemyGO.GetComponent<EntityReferenceHolder>();

            InitEntity(referencHolder, enemyGO, gameObjectIndex);
        }

        readonly IEntityFactory _entityFactory;
        readonly GameObjectResourceManager _gameObjectResourceManager;
        uint _enemiesCreated;
    }
}