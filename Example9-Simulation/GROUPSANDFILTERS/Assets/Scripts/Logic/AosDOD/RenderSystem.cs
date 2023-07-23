// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.AosDOD
{
    public static class RenderSystem
    {
        private static Transform[] transformPool = new Transform[Data.MaxVehicleCount];
        private static MeshRenderer[] meshPool = new MeshRenderer[Data.MaxVehicleCount];
        private static Material[] materials;
        private static GameObject prefab;

        public static void Initialize(GameObject _prefab, Material[] _materials)
        {
            prefab = _prefab;
            materials = _materials;
            
            for (var i = 0; i < transformPool.Length; i++)
            {
                transformPool[i] = GameObject.Instantiate(_prefab).transform;
                meshPool[i] = transformPool[i].GetComponent<MeshRenderer>();
            }
        }

        public static void Clear()
        {
            for (var i = 0; i < transformPool.Length; i++)
            {
                GameObject.Destroy(transformPool[i].gameObject);
            }
        }

        public static void Run()
        {
            if (meshPool[0] == null)
            {
                for (var i = 0; i < transformPool.Length; i++)
                {
                    transformPool[i] = GameObject.Instantiate(prefab).transform;
                    meshPool[i] = transformPool[i].GetComponent<MeshRenderer>();
                }
            }

            for (var i = 0; i < Data.Vehicles.Length; i++)
            {
                ref var vehicle = ref Data.Vehicles[i];
                if (!vehicle.IsAlive)
                {
                    meshPool[i].enabled = false;
                    continue;
                }

                var position = vehicle.Position;
                transformPool[i].position = new Vector3(position.x, 0, position.y);
                meshPool[i].enabled = true;
                meshPool[i].material = materials[vehicle.Team];
            }
        }
    }
}