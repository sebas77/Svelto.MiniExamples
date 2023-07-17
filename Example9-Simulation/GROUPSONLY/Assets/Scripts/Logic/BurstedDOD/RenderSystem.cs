// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.BurstedDOD
{
    public static class RenderSystem
    {
        private static Transform[] transformPool = new Transform[Data.MaxVehicleCount];
        private static MeshRenderer[] meshPool = new MeshRenderer[Data.MaxVehicleCount];
        private static GameObject prefab;
        private static Material[] materials;

        public static void Initialize(GameObject _prefab, Material[] _materials)
        {
            prefab = _prefab;
            materials = _materials;

            for (var i = 0; i < transformPool.Length; i++)
            {
                transformPool[i] = ((GameObject)GameObject.Instantiate(prefab)).transform;
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
        
        public static void Run(ref Data data)
        {
            for (var i = 0; i < data.VehiclePositions.Length; i++)
            {
                if (!data.VehicleAliveStatuses[i])
                {
                    meshPool[i].enabled = false;
                    continue;
                }

                var position = data.VehiclePositions[i];
                transformPool[i].position = new Vector3(position.x, 0, position.y);
                meshPool[i].enabled = true;
                meshPool[i].material = materials[data.VehicleTeams[i]];
            }
        }
    }
}