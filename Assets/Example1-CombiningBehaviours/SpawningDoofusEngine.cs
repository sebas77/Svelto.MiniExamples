using System.Collections;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;
using Svelto.Tasks.ExtraLean;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class SpawningDoofusEngine : IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            SpawningDoofuses().Run();
        }

        IEnumerator SpawningDoofuses()
        {
            var wait = new WaitForSecondsEnumerator(1);
            
            while (_numberOfDoofuses < 10)
            {
                SpawnUnityECSEntity();

                _factory.BuildEntity<DoofusEntityDescriptor>
                    (_numberOfDoofuses, GameGroups.Doofuses);
                
                _numberOfDoofuses++;

                while (wait.MoveNext()) yield return Yield.It;
            }
        }

        void SpawnUnityECSEntity()
        {
            var manager = World.Active.GetOrCreateManager<EntityManager>();
            var archetype = manager.CreateArchetype(typeof(Position),
                                                    typeof(Rotation),
                                                    typeof(RenderMesh));

//                _unityECSGroup = manager.CreateComponentGroup(archetype.ComponentTypes);

            var renderer = new RenderMesh()
            {
                castShadows    = ShadowCastingMode.On,
                receiveShadows = true,
                mesh           = _mesh,
                material       = _material
            };

            var entity = manager.CreateEntity(archetype);
            manager.SetSharedComponentData(entity, renderer);
        }

        readonly Mesh           _mesh;
        readonly Material       _material;
        readonly IEntityFactory _factory;
        int                     _numberOfDoofuses;

        public SpawningDoofusEngine(Mesh mesh, Material material, IEntityFactory factory)
        {
            _material = material;
            _factory = factory;
            _mesh     = mesh;
        }
    }
}