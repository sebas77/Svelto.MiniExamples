using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Example.OOPAbstraction.OOPLayer;
using Svelto.ECS.Schedulers.Unity;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.WithOOPLayer
{
    public class MainCompositionRootWithOOPLayer : ICompositionRoot
    {
        public void OnContextCreated<T>(T contextHolder) { }

        public void OnContextDestroyed()
        {
            //final clean up
            _enginesRoot?.Dispose();
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
            CompositionRoot<T>(out var oopManager);
            CreateStartupEntities(oopManager);
        }

        EnginesRoot _enginesRoot;

        void CompositionRoot<T>(out IOOPManager oopManager)
        {
            var unityEntitySubmissionScheduler = new UnityEntitiesSubmissionScheduler("oop-abstraction");
            _enginesRoot = new EnginesRoot(unityEntitySubmissionScheduler);
            
            var moveCubesEngine     = new MoveCubesEngine();
            var moveSpheresEngine   = new MoveSpheresEngine();
            var selectParentEngine  = new SelectNewParentEngine();

            var listOfEnginesToTick = new FasterList<IStepEngine>(new IStepEngine[]
            {
                moveCubesEngine
              , moveSpheresEngine
              , selectParentEngine
            });
            var tickingEnginesGroup = new TickingEnginesGroup(listOfEnginesToTick);

            _enginesRoot.AddEngine(tickingEnginesGroup);
            _enginesRoot.AddEngine(moveCubesEngine);
            _enginesRoot.AddEngine(moveSpheresEngine);
            _enginesRoot.AddEngine(selectParentEngine);
            
            OOPManagerCompositionRoot.Compose(_enginesRoot, listOfEnginesToTick, out oopManager, NUMBER_OF_SPHERES);
        }

        void CreateStartupEntities(IOOPManager oopManager)
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();

            for (uint i = 0; i < NUMBER_OF_CUBES; i++)
            {
                var cubeIndex = oopManager.RegisterCube();

                var cubeInit =
                    entityFactory.BuildEntity<PrimitiveEntityDescriptor>(
                        new EGID(i, ExampleGroups.CubePrimitive.BuildGroup));

                cubeInit.Init(new TransformComponent(new Vector3(i * 1.5f, 0, 0)));
                cubeInit.Init(new ObjectIndexComponent(cubeIndex));
            }

            for (uint i = 0; i < NUMBER_OF_SPHERES; i++)
            {
                var sphereIndex = oopManager.RegisterSphere();

                var sphereInit =
                    entityFactory.BuildEntity<PrimitiveEntityDescriptorWithParent>(
                        new EGID(NUMBER_OF_CUBES + i, ExampleGroups.SpherePrimitive.BuildGroup));
                
                sphereInit.Init(new TransformComponent(new Vector3(1.5f, 0, 0)));
                sphereInit.Init(new ObjectIndexComponent(sphereIndex));
            }
        }
        
        const uint NUMBER_OF_CUBES   = 5;
        const uint NUMBER_OF_SPHERES = 10; 
    }
}