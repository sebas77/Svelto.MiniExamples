// Copyright (c) Sean Nowotny

using System;
using DefaultEcs;
using DefaultEcs.System;
using Logic.DefaultECS.Components;
using UnityEngine;
using World = DefaultEcs.World;

namespace Logic.DefaultECS
{
    public class RenderSystem : ISystem<float>
    {
        private static Transform[] transformPool = new Transform[Data.MaxVehicleCount];
        private static MeshRenderer[] meshPool = new MeshRenderer[Data.MaxVehicleCount];
        private readonly GameObject prefab;
        private readonly World world;
        private readonly EntitySet colorfulEntitiesSet;
        private readonly EntitySet sirenLightEntitiesSet;
        private readonly Material[] materials;
        private readonly Material sirenLightMaterial;

        public bool IsEnabled { get; set; } = true;

        public RenderSystem(World _world, GameObject _prefab, Material[] _materials, Material _sirenLightMaterial)
        {
            world = _world;
            prefab = _prefab;
            materials = _materials;
            sirenLightMaterial = _sirenLightMaterial;

            colorfulEntitiesSet = world.GetEntities().With<PositionDC>().Without<SirenLight>().AsSet();
            sirenLightEntitiesSet = world.GetEntities().With<PositionDC>().With<SirenLight>().AsSet();

            Initialize();
        }
        
        public void Initialize()
        {
            for (var i = 0; i < transformPool.Length; i++)
            {
                transformPool[i] = GameObject.Instantiate(prefab).transform;
                meshPool[i] = transformPool[i].GetComponent<MeshRenderer>();
            }
        }

        public void Clear()
        {
            for (var i = 0; i < transformPool.Length; i++)
            {
                GameObject.Destroy(transformPool[i].gameObject);
            }
        }

        public void Update(float state)
        {
            var colorfulEntities = colorfulEntitiesSet.GetEntities();
            var colorfulEntitiesLength = colorfulEntities.Length;

            var sirenLightEntities = sirenLightEntitiesSet.GetEntities();

            UpdateVehicleColors(colorfulEntities, false, 0);
            UpdateVehicleColors(sirenLightEntities, true, colorfulEntitiesLength);

            var entitiesCount = colorfulEntities.Length + sirenLightEntities.Length;
            for (var i = entitiesCount; i < Data.MaxVehicleCount; i++)
            {
                meshPool[i].enabled = false;
            }
        }

        private void UpdateVehicleColors(ReadOnlySpan<Entity> entities, bool sirenLightLit, int poolStartIndex)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                int poolIndex = poolStartIndex + i;

                var position = entities[i].Get<PositionDC>().Value;
                transformPool[poolIndex].position = new Vector3(position.x, 0, position.y);
                meshPool[poolIndex].enabled = true;

                if (sirenLightLit)
                {
                    // TODO: Light intensity
                    meshPool[poolIndex].material = sirenLightMaterial;
                }
                else
                {
                    meshPool[poolIndex].material = materials[entities[i].Get<TeamDC>().Value];
                }
            }
        }

        public void Dispose()
        {
        }
    }
}