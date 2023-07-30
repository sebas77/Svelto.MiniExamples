#if DEBUG && !PROFILE_SVELTO
#define ENABLE_INSPECTOR
#endif

using System.Threading.Tasks;
using Svelto.Context;
using Svelto.ECS;
using Svelto.ECS.Schedulers;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace Logic.SveltoECS
{
    public class SirensSequentialEngines: UnsortedEnginesGroup<IStepEngine<float>, float> { }

    public class SirensCompositionRoot: ICompositionRoot
    {
        public void OnContextInitialized<T>(T contextHolder)
        {
            var sveltoContext = (contextHolder as SveltoContext);
            
            _prefab = sveltoContext.prefab;
            _materials = sveltoContext.materials;
            _sirenLightMaterial = sveltoContext.sirenLightMaterial;
            
            _ticker = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_ticker);
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();
            var entityFactory = _enginesRoot.GenerateEntityFactory();

            _sequentialEnginesGroup = new SirensSequentialEngines();

            _sequentialEnginesGroup.Add(new SpawnVehiclesSystem(entityFactory));
            _sequentialEnginesGroup.Add(new EnemyTargetSystem());
            _sequentialEnginesGroup.Add(new VehicleMovementSystem());
            _sequentialEnginesGroup.Add(new SwitchSirenLightOnSystem());
            _sequentialEnginesGroup.Add(new SwitchSirenLightOffSystem());
            _sequentialEnginesGroup.Add(new DecrementTimersSystem());
            _sequentialEnginesGroup.Add(new ShootSystem());
            _sequentialEnginesGroup.Add(new DieSystem(entityFunctions));
            _sequentialEnginesGroup.Add(new RenderSystem(_prefab, _materials, _sirenLightMaterial));

            _enginesRoot.AddEngine(_sequentialEnginesGroup);
#if ENABLE_INSPECTOR            
            SveltoInspector.Attach(_enginesRoot);
#endif

            MainLoop();
        }

        public void OnContextDestroyed(bool hasBeenInitialised)
        {
            _disposed = true;
            _enginesRoot.Dispose();
        }

        public void OnContextCreated<T>(T contextHolder) { }

        async void MainLoop()
        {
            while (Application.isPlaying && _disposed == false)
            {
                _ticker.SubmitEntities();

                _sequentialEnginesGroup.Step(Time.deltaTime);

                if (Input.GetKeyUp(KeyCode.Space))
                {
//                    Data.EnableRendering = !Data.EnableRendering;
//                    if (Data.EnableRendering)
//                    {
//                        renderSystem.Initialize();
//                    }
//                    else
//                    {
//                        renderSystem.Clear();
//                    }
                }

//                if (Data.EnableRendering)
//                {
//                    renderSystem.Update(Time.deltaTime);
//                }

                await Task.Yield();
            }
        }
        
        SimpleEntitiesSubmissionScheduler _ticker;
        SirensSequentialEngines _sequentialEnginesGroup;
        GameObject _prefab;
        Material[] _materials;
        Material _sirenLightMaterial;
        EnginesRoot _enginesRoot;
        bool _disposed;
    }
}