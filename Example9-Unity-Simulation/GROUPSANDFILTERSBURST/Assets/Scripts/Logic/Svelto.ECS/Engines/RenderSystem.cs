using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS;
using UnityEngine;

namespace Logic.SveltoECS
{
    public class RenderSystem: IQueryingEntitiesEngine, IStepEngine<float>, IDisposable
    {
        static Transform[] transformPool = new Transform[Data.MaxVehicleCount];
        static MeshRenderer[] meshPool = new MeshRenderer[Data.MaxVehicleCount];

        public RenderSystem(GameObject prefab, Material[] materials, Material sirenLightMaterial)
        {
            _prefab = prefab;
            _materials = materials;
            _sirenLightMaterial = sirenLightMaterial;

            Initialize();
        }

        public void Initialize()
        {
            for (var i = 0; i < transformPool.Length; i++)
            {
                transformPool[i] = GameObject.Instantiate(_prefab).transform;
                meshPool[i] = transformPool[i].GetComponent<MeshRenderer>();
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < transformPool.Length; i++)
            {
                var transform = transformPool[i];
                if (transform)
                    GameObject.Destroy(transform);
            }
        }

        public void Step(in float time)
        {
            int entitiesCount = 0;
            var filters = entitiesDB.GetFilters();
            foreach (var (indicies, group) in filters.GetPersistentFilter<PositionDC>(VechilesFilterIds.VehiclesWithSirenOff))
            {
                var (entitiesWithSirenOff, _) = entitiesDB.QueryEntities<PositionDC>(group);
                UpdateVehicleColors(entitiesWithSirenOff, entitiesCount, indicies, VehicleTag.Offset(group));

                entitiesCount += indicies.count;
            }
            
            foreach (var (indicies, group) in filters.GetPersistentFilter<PositionDC>(VechilesFilterIds.VehiclesWithSirenOn))
            {
                var (entitiesWithSirenOn, _) = entitiesDB.QueryEntities<PositionDC>(group);

                UpdateSirenVehicleColors(entitiesWithSirenOn, entitiesCount, indicies);

                entitiesCount += indicies.count;
            }
            
            for (var i = entitiesCount; i < Data.MaxVehicleCount; i++)
            {
                meshPool[i].enabled = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateVehicleColors(NB<PositionDC> entities, int poolIndexCount, EntityFilterIndices indicies, uint teamIndex)
        {
            var entitiesLengthCount = indicies.count;
            
            for (var i = 0; i < entitiesLengthCount; i++)
            {
                int poolIndex = poolIndexCount + i;

                ref var position = ref entities[indicies[i]].Value;
                
                transformPool[poolIndex].position = new Vector3(position.x, 0, position.y);
                var meshRenderer = meshPool[poolIndex];
                meshRenderer.enabled = true;
                meshRenderer.material = _materials[teamIndex];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateSirenVehicleColors(NB<PositionDC> entities, int poolIndexCount, EntityFilterIndices indicies)
        {
            var entitiesLengthCount = indicies.count;
            
            for (var i = 0; i < entitiesLengthCount; i++)
            {
                int poolIndex = poolIndexCount + i;

                ref var position = ref entities[indicies[i]].Value;
                
                transformPool[poolIndex].position = new Vector3(position.x, 0, position.y);
                var meshRenderer = meshPool[poolIndex];
                meshRenderer.enabled = true;
                meshRenderer.material = _sirenLightMaterial;
            }
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(RenderSystem);

        readonly GameObject _prefab;
        readonly Material[] _materials;
        readonly Material _sirenLightMaterial;
    }
}