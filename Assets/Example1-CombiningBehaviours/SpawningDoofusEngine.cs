using System.Collections;
using Svelto.ECS.Components;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks;
using Svelto.Tasks.ExtraLean;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class SpawningDoofusEngine : IQueryingEntitiesEngine
    {
        public SpawningDoofusEngine(Mesh mesh, Material material, IEntityFactory factory)
        {
            _material = material;
            _factory = factory;
            _mesh     = mesh;
        }
        
        public IEntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            SpawningDoofuses().RunOn(StandardSchedulers.coroutineScheduler);
        }
        
        IEnumerator SpawningDoofuses()
        {
            while (_numberOfDoofuses < 10000)
            {
                EntityStructInitializer init;
                
                init = _factory.BuildEntity<DoofusEntityDescriptor>(_numberOfDoofuses, Random.value < 0.5f ? GameGroups.DOOFUSESMOVING : GameGroups.DOOFUSESJUMPING);

                var positionEntityStruct = new PositionEntityStruct
                {
                    position = new ECSVector3(Random.value * 40, 0, Random.value * 40)
                };
                init.Init(positionEntityStruct);
                
                SpawnUnityECSEntity(ref positionEntityStruct);
                
                _numberOfDoofuses++;

                yield return Yield.It;
            }
        }

        void SpawnUnityECSEntity(ref PositionEntityStruct positionEntityStruct)
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
            manager.SetComponentData(entity, new Position() { Value = new float3(positionEntityStruct.position.x,
                                                                                 positionEntityStruct.position.y,
                                                                                 positionEntityStruct.position.z)});
            
            manager.SetSharedComponentData(entity, renderer);
        }

        readonly Mesh           _mesh;
        readonly Material       _material;
        readonly IEntityFactory _factory;
        int                     _numberOfDoofuses;
    }
}