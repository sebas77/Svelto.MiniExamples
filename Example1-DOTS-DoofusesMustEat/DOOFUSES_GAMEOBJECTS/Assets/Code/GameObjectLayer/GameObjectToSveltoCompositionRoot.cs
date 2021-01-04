using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;

namespace Svelto.ECS.MiniExamples.GameObjectsLayer
{
    public static class GameObjectToSveltoCompositionRoot
    {
        public static void Compose
        (EnginesRoot enginesRoot, FasterList<IJobifiedEngine> enginesToTick
       , out GameObjectManager gameObjectManager)
        {
            var goManager = new GameObjectManager();

            enginesRoot.AddEngine(new InstantiateGameObjectOnSveltoEntityEngine(goManager));
            AddSveltoEngineToTick(new RenderingGameObjectSynchronizationEngine(goManager), enginesRoot, enginesToTick);

            gameObjectManager = goManager;
        }

        static void AddSveltoEngineToTick
            (IJobifiedEngine engine, EnginesRoot enginesRoot, FasterList<IJobifiedEngine> enginesToTick)
        {
            enginesRoot.AddEngine(engine);
            enginesToTick.Add(engine);
        }
    }
}