using Svelto.ECS.Schedulers.Unity;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class Bootstrap : MonoBehaviour 
    {
        [SerializeField]
        Mesh mesh;
    
        [SerializeField]
        Material material;
        void OnDestroy()
        {
            _enginesRoot.Dispose();
        }

        void Start()
        {
            QualitySettings.vSyncCount = -1;
            Application.targetFrameRate = 120;
            
            UnityECS();
            SveltoECS();
        }

        void SveltoECS()
        {
            //creates the _enginesRoot, for more information read my svelto.ECS articles
            _enginesRoot = new EnginesRoot(new UnityEntitySubmissionScheduler());
            
            
            //create a synchronization enumerator to be used inside the Svelto.Tasks
            ThreadSynchronizationSignal _signal = new ThreadSynchronizationSignal("name");
            
            //add the engines we are going to use
            _enginesRoot.AddEngine(new RenderingDataSyncronizationEngine(_signal));
        }

        void UnityECS()
        {
            var manager = World.Active.GetOrCreateManager<EntityManager>();
            var archetype = manager.CreateArchetype(typeof(Position),
                                                    typeof(Rotation),
                                                    typeof(RenderMesh));
            
            var renderer = new RenderMesh()
            {
                castShadows    = ShadowCastingMode.On,
                receiveShadows = true,
                mesh           = mesh,
                material       = material
            };
            
            {
                var entity = manager.CreateEntity(archetype);
                manager.SetSharedComponentData(entity, renderer);
            }
        }

        EnginesRoot    _enginesRoot;
    }
}