using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.HUD;

namespace Svelto.ECS.Example.Survive
{
    public static class HudLayerContext
    {
        /// <summary>
        /// HudLayer is the only layer in this example that will interface with gameobject using the Svelto Implementors
        /// technique. Which is fine for simple GUI like ths one.
        /// </summary>
        public static void Setup(IEntityStreamConsumerFactory entityStreamConsumerFactory,
            FasterList<IStepEngine> unorderedEngines, FasterList<IStepEngine> orderedEngines, EnginesRoot enginesRoot)
        {
            //hud engines
            var hudEngine = new HUDEngine(entityStreamConsumerFactory);
            var scoreEngine = new UpdateScoreEngine(entityStreamConsumerFactory);
            var restartGameOnPlayerDeath = new RestartGameOnPlayerDeathEngine();

            enginesRoot.AddEngine(hudEngine);
            enginesRoot.AddEngine(scoreEngine);

            unorderedEngines.Add(hudEngine);
            orderedEngines.Add(scoreEngine);
            
            unorderedEngines.Add(restartGameOnPlayerDeath);
        }
    }
}