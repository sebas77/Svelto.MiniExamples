using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.Camera
{
    public static class CameraLayerContext
    {
        public static void Setup(FasterList<IStepEngine> unorderedEngines, EnginesRoot enginesRoot)
        {
            var cameraFollowTargetEngine = new CameraFollowingTargetEngine();
            enginesRoot.AddEngine(cameraFollowTargetEngine);
            unorderedEngines.Add(cameraFollowTargetEngine);
        }
    }
}