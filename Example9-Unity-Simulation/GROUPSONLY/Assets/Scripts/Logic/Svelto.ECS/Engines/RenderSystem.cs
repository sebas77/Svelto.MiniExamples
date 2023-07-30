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
                GameObject.Destroy(transformPool[i].gameObject);
            }
        }

        public void Step(in float time)
        {
            int entitiesCount = 0;
            foreach (var ((entitiesWithSirenOff, count), group) in entitiesDB.QueryEntities<PositionDC>(VehicleSirenOff.Groups))
            {
                UpdateVehicleColors(entitiesWithSirenOff, entitiesCount, count, VehicleSirenOff.Offset(group));

                entitiesCount += count;
            }
            
            foreach (var ((entitiesWithSirenOn, count), _) in entitiesDB.QueryEntities<PositionDC>(VehicleSirenOn.Groups))
            {
                UpdateSirenVehicleColors(entitiesWithSirenOn, entitiesCount, count);

                entitiesCount += count;
            }

            for (var i = entitiesCount; i < Data.MaxVehicleCount; i++)
            {
                meshPool[i].enabled = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateVehicleColors(NB<PositionDC> entities, int poolIndexCount, int entitiesLength, uint teamIndex)
        {
            for (var i = 0; i < entitiesLength; i++)
            {
                int poolIndex = poolIndexCount + i;

                ref var position = ref entities[i].Value;
                transformPool[poolIndex].position = new Vector3(position.x, 0, position.y);
                meshPool[poolIndex].enabled = true;
                meshPool[poolIndex].material = _materials[teamIndex];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateSirenVehicleColors(NB<PositionDC> entities, int poolIndexCount, int entitiesLength)
        {
            for (var i = 0; i < entitiesLength; i++)
            {
                int poolIndex = poolIndexCount + i;

                ref var position = ref entities[i].Value;
                if (poolIndex >= Data.MaxVehicleCount)
                    throw new Exception("poolIndex >= Data.MaxVehicleCount");
                
                transformPool[poolIndex].position = new Vector3(position.x, 0, position.y);
                meshPool[poolIndex].enabled = true;
                meshPool[poolIndex].material = _sirenLightMaterial;
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