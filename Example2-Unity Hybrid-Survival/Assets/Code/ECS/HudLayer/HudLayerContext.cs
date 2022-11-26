using Svelto.DataStructures;
using Svelto.ECS.Example.Survive.HUD;

namespace Svelto.ECS.Example.Survive
{
    public static class HudLayerContext
    {
        public static void HudLayerSetup(IEntityStreamConsumerFactory entityStreamConsumerFactory,
            FasterList<IStepEngine> unorderedEngines, FasterList<IStepEngine> orderedEngines, EnginesRoot enginesRoot)
        {
            //hud and sound engines
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