using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers.Unity;
using Vector3 = UnityEngine.Vector3;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public class MainCompositionRootWithOOPLayer : ICompositionRoot
    {
        public void OnContextInitialized<T>(T contextHolder) { CompositionRoot<T>(); }
        public void OnContextDestroyed()
        {
            //final clean up
            _enginesRoot?.Dispose();
        }

        public void OnContextCreated<T>(T contextHolder) { }

        void CompositionRoot<T>()
        {
            var unityEntitySubmissionScheduler = new UnityEntitiesSubmissionScheduler("oop-abstraction");
            _enginesRoot = new EnginesRoot(unityEntitySubmissionScheduler);

            var oopManager = new OOPManager();

            CreateStartupEntities(oopManager);

            var moveCubesEngine     = new MoveCubesEngine();
            var moveSpheresEngine   = new MoveSpheresEngine();
            var selectParentEngine  = new SelectNewParentEngine();
            var syncEngine          = new SyncTransformEngine(oopManager);
            var syncHierarchyEngine = new SyncHierarchyEngine(oopManager);

            TickingEnginesGroup tickingEnginesGroup = new TickingEnginesGroup(
                new FasterList<ITickingEngine>(new ITickingEngine[]
                {
                    moveCubesEngine
                  , moveSpheresEngine
                  , selectParentEngine
                  , syncEngine
                  , syncHierarchyEngine
                }));

            _enginesRoot.AddEngine(tickingEnginesGroup);
            _enginesRoot.AddEngine(moveCubesEngine);
            _enginesRoot.AddEngine(moveSpheresEngine);
            _enginesRoot.AddEngine(selectParentEngine);
            _enginesRoot.AddEngine(syncEngine);
            _enginesRoot.AddEngine(syncHierarchyEngine);
        }

        void CreateStartupEntities(OOPManager oopManager)
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();

            for (uint i = 0; i < 5; i++)
            {
                var cubeIndex = oopManager.RegisterCube();

                var cubeInit =
                    entityFactory.BuildEntity<PrimitiveEntityDescriptor>(
                        new EGID(i, ExampleGroups.CubePrimitive.BuildGroup));

                cubeInit.Init(new TransformComponent(new Vector3(i * 1.5f, 0, 0)));
                cubeInit.Init(new OOPIndexComponent(cubeIndex));
            }

            var sphereIndex = oopManager.RegisterSphere();

            var sphereInit =
                entityFactory.BuildEntity<PrimitiveEntityDescriptor>(
                    new EGID(6, ExampleGroups.SpherePrimitive.BuildGroup));

            sphereInit.Init(new TransformComponent(new Vector3(1.5f, 0, 0)));
            sphereInit.Init(new OOPIndexComponent(sphereIndex));
        }

        EnginesRoot _enginesRoot;
    }
}