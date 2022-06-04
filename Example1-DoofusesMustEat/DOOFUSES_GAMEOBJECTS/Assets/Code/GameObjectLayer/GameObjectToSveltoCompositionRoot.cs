using Svelto.DataStructures;
using Svelto.ECS.MiniExamples.Doofuses.GameObjects.GameobjectLayer;
using Svelto.ECS.SveltoOnDOTS;

namespace Svelto.ECS.Miniexamples.Doofuses.GameObjectsLayer
{
    public static class GameObjectToSveltoCompositionRoot
    {
        public static void Compose
            (EnginesRoot enginesRoot, FasterList<IJobifiedEngine> enginesToTick, ECSGameObjectsEntityManager goManager)
        {
            enginesRoot.AddEngine(new InstantiateGameObjectOnSveltoEntityEngine(goManager));
            AddSveltoEngineToTick(new RenderingGameObjectSynchronizationEngine(goManager), enginesRoot, enginesToTick);
        }

        static void AddSveltoEngineToTick
            (IJobifiedEngine engine, EnginesRoot enginesRoot, FasterList<IJobifiedEngine> enginesToTick)
        {
            enginesRoot.AddEngine(engine);
            enginesToTick.Add(engine);
        }
    }
}