// Copyright (c) Sean Nowotny

using Arch.Core;
using Arch.System;
using Logic.Arch.Components;
using UnityEngine;

namespace Logic.Arch.Systems
{
    public class RenderSystem : ISystem<float>
    {
        private static Transform[] transformPool = new Transform[Data.MaxVehicleCount];
        private static MeshRenderer[] meshPool = new MeshRenderer[Data.MaxVehicleCount];
        private readonly GameObject prefab;
        private readonly World world;
        private readonly Material[] materials;
        private QueryDescription query;

        public RenderSystem(World _world, GameObject _prefab, Material[] _materials)
        {
            world = _world;
            prefab = _prefab;
            materials = _materials;
        }

        public void Initialize()
        {
            query = new QueryDescription().WithAll<PositionDC>();
            
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

        public void Update(in float state)
        {
            var entitiesCount = world.CountEntities(query);
            var nonce = new Entity[entitiesCount];
            world.GetEntities(query, nonce);
            for (var i = 0; i < nonce.Length; i++)
            {
                var position = world.Get<PositionDC>(nonce[i]).Value;
                transformPool[i].position = new Vector3(position.x, 0, position.y);
                meshPool[i].enabled = true;
                meshPool[i].material = materials[world.Get<TeamDC>(nonce[i]).Value];
            }

            for (var i = entitiesCount; i < Data.MaxVehicleCount; i++)
            {
                meshPool[i].enabled = false;
            }
        }

        public void Dispose()
        {
        }

        public void BeforeUpdate(in float t)
        {
        }

        public void AfterUpdate(in float t)
        {
        }
    }
}