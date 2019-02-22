using System.Collections;
using Svelto.ECS.EntityStructs;
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
    public class SpawningDoofusEngine : SingleEntityEngine<PositionEntityStruct>, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            SpawningDoofuses().Run();
        }
        
        protected override void Add(ref PositionEntityStruct entityView)
        {
            entityView.position.x = Random.value * 10;
            entityView.position.z = Random.value * 10;
        }

        IEnumerator SpawningDoofuses()
        {
            var wait = new WaitForSecondsEnumerator(1);
            
            while (_numberOfDoofuses < 10)
            {
                SpawnUnityECSEntity();

                var init = _factory.BuildEntity<DoofusEntityDescriptor>(_numberOfDoofuses, GameGroups.DOOFUSES);
                
            //    init.Init(new Position());
                
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

            var renderer = new RenderMesh
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

        protected override void Remove(ref PositionEntityStruct entityView)
        {
            
        }
    }
}