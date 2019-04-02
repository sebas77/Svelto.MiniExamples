using System.Collections;
using Svelto.ECS.Example.Survive.Characters;
using Svelto.ECS.Example.Survive.Characters.Enemies;
using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Example.Survive.ResourceManager;
using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public class EnemyFactory
    {
        public EnemyFactory(GameObjectFactory gameObjectFactory, IEntityFactory entityFactory)
        {
            _gameobjectFactory = gameObjectFactory;
            _entityFactory     = entityFactory;
        }

        public IEnumerator Build(EnemySpawnData enemySpawnData, EnemyAttackStruct enemyAttackstruct)
        {
            var enumerator = _gameobjectFactory.Build(enemySpawnData.enemyPrefab);
            
            yield return enumerator;
            
            GameObject enemyGO = enumerator.Current;
            
            //implementors are ONLY necessary if you need to wrap objects around entity view structs. In the case
            //of Unity, they are needed to wrap Monobehaviours and not used in any other case
            
            var implementors = enemyGO.GetComponentsInChildren<IImplementor>();

            //using the GameObject GetInstanceID() as entityID will help to directly use the result of Unity functions
            //to index the entity in the Svelto database. 
            var initializer = _entityFactory
                  .BuildEntity<EnemyEntityDescriptor>(new EGID((uint) enemyGO.GetInstanceID(), ECSGroups.ActiveEnemies),
                                                       implementors);

            //Initialize the pure EntityStructs. This should be the preferred pattern, there is much less boiler plate
            //too
            initializer.Init(enemyAttackstruct);
            initializer.Init(new HealthEntityStruct {currentHealth  = 100});
            initializer.Init(new ScoreValueEntityStruct {scoreValue = (int) (enemySpawnData.targetType + 1) * 10});
            initializer.Init(new EnemyEntityStruct {enemyType       = enemySpawnData.targetType});
            initializer.Init(new EnemySinkStruct {sinkAnimSpeed = 2.5f});

            var transform = enemyGO.transform;
            var spawnInfo = enemySpawnData.spawnPoint;

            transform.position = spawnInfo;
        }

        public void Preallocate()
        {
            _entityFactory.PreallocateEntitySpace<EnemyEntityDescriptor>(ECSGroups.ActiveEnemies, 10);
        }
        
        readonly IEntityFactory    _entityFactory;
        readonly GameObjectFactory _gameobjectFactory;
    }
}