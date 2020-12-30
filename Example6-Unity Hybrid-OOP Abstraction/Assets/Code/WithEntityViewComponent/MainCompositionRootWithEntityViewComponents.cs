using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers.Unity;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.EntityViewComponents
{
    public class MainCompositionRootWithEntityViewComponents : ICompositionRoot
    {
        public MainCompositionRootWithEntityViewComponents() { QualitySettings.vSyncCount = 1; }

        public void OnContextCreated<T>(T contextHolder) { }

        public void OnContextDestroyed()
        {
            //final clean up
            _enginesRoot?.Dispose();
        }

        public void OnContextInitialized<T>(T contextHolder) { CompositionRoot(); }

        void CompositionRoot()
        {
            var unityEntitySubmissionScheduler = new UnityEntitiesSubmissionScheduler("oop-abstraction");
            _enginesRoot = new EnginesRoot(unityEntitySubmissionScheduler);

            CreateStartupEntities();

            var moveCubesEngine    = new MoveCubesEngine();
            var moveSpheresEngine  = new MoveSpheresEngine();
            var selectParentEngine = new SelectNewParentEngine();

            var tickingEnginesGroup = new TickingEnginesGroup(
                new FasterList<IStepEngine>(new IStepEngine[]
                {
                    moveCubesEngine, moveSpheresEngine, selectParentEngine
                }));

            _enginesRoot.AddEngine(tickingEnginesGroup);
            _enginesRoot.AddEngine(moveCubesEngine);
            _enginesRoot.AddEngine(moveSpheresEngine);
            _enginesRoot.AddEngine(selectParentEngine);
        }

        void CreateStartupEntities()
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();

            TransformImplementor cubeImplementor = default;
            for (uint i = 0; i < 5; i++)
            {
                var cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubeImplementor          = cubeObject.AddComponent<TransformImplementor>();
                cubeImplementor.position = new Vector3(i * 1.5f, 0, 0);

                entityFactory.BuildEntity<PrimitiveEntityDescriptor>(new EGID(i, ExampleGroups.CubeGroup)
                                                                   , new[] {cubeImplementor});
            }

            var sphereObject      = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var sphereImplementor = sphereObject.AddComponent<TransformImplementor>();

            entityFactory.BuildEntity<PrimitiveEntityDescriptor>(new EGID(6, ExampleGroups.SphereGroup)
                                                               , new[] {sphereImplementor});

            sphereImplementor.transform.parent = cubeImplementor.transform;
        }

        EnginesRoot _enginesRoot;
    }
}