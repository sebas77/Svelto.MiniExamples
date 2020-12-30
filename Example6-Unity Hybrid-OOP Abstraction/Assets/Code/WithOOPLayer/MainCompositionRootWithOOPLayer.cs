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
            CompositionRoot<T>();
            CreateStartupEntities();
        }

        void CompositionRoot<T>()
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
            
            OOPManagerCompositionRoot.Compose(_enginesRoot, listOfEnginesToTick, NUMBER_OF_SPHERES);
        }

        void CreateStartupEntities()
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();

            for (uint i = 0; i < NUMBER_OF_CUBES; i++)
            {
                var cubeInit = entityFactory.BuildEntity<PrimitiveEntityDescriptor>(
                        new EGID(i, ExampleGroups.CubePrimitive.BuildGroup));

                cubeInit.Init(new TransformComponent(new Vector3(i * 1.5f, 0, 0)));
                cubeInit.Init(new ObjectIndexComponent(PrimitiveType.Cube));
            }

            for (uint i = 0; i < NUMBER_OF_SPHERES; i++)
            {
                var sphereInit =
                    entityFactory.BuildEntity<PrimitiveEntityWithParentDescriptor>(
                        new EGID(NUMBER_OF_CUBES + i, ExampleGroups.SpherePrimitive.BuildGroup));
                
                sphereInit.Init(new TransformComponent(new Vector3(1.5f, 0, 0)));
                sphereInit.Init(new ObjectIndexComponent(PrimitiveType.Sphere));
            }
        }
        
        const uint NUMBER_OF_CUBES   = 5;
        const uint NUMBER_OF_SPHERES = 10;
        
        EnginesRoot _enginesRoot;
    }
}