using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.Camera
{
    public static class CameraLayerContext
    {
        public static void CameraLayerSetup(ITime time, FasterList<IStepEngine> unorderedEngines, EnginesRoot enginesRoot)
        {
            var cameraFollowTargetEngine = new CameraFollowingTargetEngine(time);
            enginesRoot.AddEngine(cameraFollowTargetEngine);
            unorderedEngines.Add(cameraFollowTargetEngine);
        }
    }
}