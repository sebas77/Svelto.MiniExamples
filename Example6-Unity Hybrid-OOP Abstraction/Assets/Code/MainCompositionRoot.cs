using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers.Unity;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction
{
    public class MainCompositionRoot : ICompositionRoot
    {
        public MainCompositionRoot() { QualitySettings.vSyncCount = 1; }

        public void OnContextCreated<T>(T contextHolder) { }

        public void OnContextInitialized<T>(T contextHolder) { CompositionRoot(contextHolder as UnityContext); }

        public void OnContextDestroyed()
        {
            //final clean up
            _enginesRoot.Dispose();
        }

        void CompositionRoot(UnityContext contextHolder)
        {
            var unityEntitySubmissionScheduler = new UnityEntitiesSubmissionScheduler("oop-abstraction");
            _enginesRoot = new EnginesRoot(unityEntitySubmissionScheduler);

            CreateStartupEntities();

            var moveCubesEngine    = new MoveCubesEngine();
            var moveSpheresEngine  = new MoveSpheresEngine();
            var selectParentEngine = new SelectNewParentEngine();
            
            TickingEnginesGroup tickingEnginesGroup =
                new TickingEnginesGroup(new FasterList<ITickingEngine>(new ITickingEngine[] {moveCubesEngine, moveSpheresEngine, selectParentEngine}));
            
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
                cubeImplementor = cubeObject.AddComponent<TransformImplementor>();
                cubeImplementor.position = new Vector3(i * 1.5f, 0, 0);

                entityFactory.BuildEntity<CubeEntityDescriptor>(new EGID(i, ExampleGroups.CubeGroup)
                                                              , new[] {cubeImplementor});
                
                
            }

            var sphereObject      = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var sphereImplementor = sphereObject.AddComponent<TransformImplementor>();

            entityFactory.BuildEntity<CubeEntityDescriptor>(new EGID(6, ExampleGroups.SphereGroup)
                                                          , new[] {sphereImplementor});

            sphereImplementor.transform.parent = cubeImplementor.transform;
        }
        
        EnginesRoot _enginesRoot;
    }
}