using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.OOPLayer;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public static class EnemyLayerContext
    {
        public static void EnemyLayerSetup(GameObjectFactory gameObjectFactory, IEntityFactory entityFactory,
            IEntityStreamConsumerFactory entityStreamConsumerFactory, ITime time, IEntityFunctions entityFunctions,
            FasterList<IStepEngine> unorderedEngines, FasterList<IStepEngine> orderedEngines,
            WaitForSubmissionEnumerator waitForSubmissionEnumerator, EnginesRoot enginesRoot)
        {
            //Factory is one of the few OOP patterns that work very well with ECS. Its use is highly encouraged
            var enemyFactory = new EnemyFactory(gameObjectFactory, entityFactory);
//Enemy related engines
            var enemyAnimationEngine = new EnemyChangeAnimationOnPlayerDeathEngine();
            var enemyDamageFXEngine = new EnemySpawnEffectOnDamageEngine(entityStreamConsumerFactory);
            var enemyAttackEngine = new EnemyAttackEngine(time);
            var enemyMovementEngine = new EnemyMovementEngine();
//Spawner engines are factories engines that can build entities
            var enemySpawnerEngine = new EnemySpawnerEngine(enemyFactory, entityFunctions);
            var enemyDeathEngine = new EnemyDeathEngine(
                entityFunctions,
                entityStreamConsumerFactory,
                time,
                waitForSubmissionEnumerator);

//enemy engines
            enginesRoot.AddEngine(enemySpawnerEngine);
            enginesRoot.AddEngine(enemyAttackEngine);
            enginesRoot.AddEngine(enemyMovementEngine);
            enginesRoot.AddEngine(enemyAnimationEngine);
            enginesRoot.AddEngine(enemyDeathEngine);
            enginesRoot.AddEngine(enemyDamageFXEngine);

            unorderedEngines.Add(enemySpawnerEngine);
            unorderedEngines.Add(enemyMovementEngine);

            orderedEngines.Add(enemyDamageFXEngine);
            orderedEngines.Add(enemyAttackEngine);
            orderedEngines.Add(enemyDeathEngine);
        }
    }
}