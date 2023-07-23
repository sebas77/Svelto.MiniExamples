// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.DOD
{
    public static class RenderSystem
    {
        private static Transform[] transformPool = new Transform[Data.MaxVehicleCount];
        private static MeshRenderer[] meshPool = new MeshRenderer[Data.MaxVehicleCount];
        private static Material[] materials;

        public static void Initialize(GameObject _prefab, Material[] _materials)
        {
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
            for (var i = 0; i < Data.VehiclePositions.Length; i++)
            {
                if (!Data.VehicleAliveStatuses[i])
                {
                    meshPool[i].enabled = false;
                    continue;
                }

                var position = Data.VehiclePositions[i];
                transformPool[i].position = new Vector3((float) position.x, 0, (float) position.y);
                meshPool[i].enabled = true;
                meshPool[i].material = materials[Data.VehicleTeams[i]];
            }
        }
    }
}