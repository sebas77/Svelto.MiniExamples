using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.OOPLayer;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public static class EnemyLayerContext
    {
        public static void Setup(IEntityFactory entityFactory, ITime time, IEntityFunctions entityFunctions,
            FasterList<IStepEngine> unorderedEngines, FasterList<IStepEngine> orderedEngines,
            WaitForSubmissionEnumerator waitForSubmissionEnumerator, EnginesRoot enginesRoot,
            GameObjectResourceManager gameObjectResourceManager)
        {
            //Factory is one of the few OOP patterns that work very well with ECS. Its use is highly encouraged
            var enemyFactory = new EnemyFactory(entityFactory, gameObjectResourceManager);
//Enemy related engines
            //var enemyAnimationEngine = new EnemyChangeAnimationOnPlayerDeathEngine();
            var enemyDamageFXEngine = new EnemySpawnEffectOnDamageEngine();
            var enemyAttackEngine = new EnemyAttackEngine(time);
            var enemyMovementEngine = new EnemyMovementEngine();
//Spawner engines are factories engines that can build entities
            var enemySpawnerEngine = new EnemySpawnerEngine(enemyFactory);
            var enemyDeathEngine = new EnemyDeathEngine(entityFunctions,
                time, waitForSubmissionEnumerator, gameObjectResourceManager);
            var enemyTargetDeadEngine = new EnemyChangeAnimationOnTargetDeathEngine();

//enemy engines
            enginesRoot.AddEngine(enemySpawnerEngine);
            enginesRoot.AddEngine(enemyAttackEngine);
            enginesRoot.AddEngine(enemyMovementEngine);
            enginesRoot.AddEngine(enemyDeathEngine);
            enginesRoot.AddEngine(enemyDamageFXEngine);
            enginesRoot.AddEngine(enemyTargetDeadEngine);

            unorderedEngines.Add(enemySpawnerEngine);
            unorderedEngines.Add(enemyMovementEngine);

            orderedEngines.Add(enemyDamageFXEngine);
            orderedEngines.Add(enemyAttackEngine);
            orderedEngines.Add(enemyDeathEngine);
        }
    }
}