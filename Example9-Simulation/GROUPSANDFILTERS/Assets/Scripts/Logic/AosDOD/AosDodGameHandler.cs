// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.AosDOD
{
    public class AosDodGameHandler : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Material[] materials;

        private void Start()
        {
            for (var i = 0; i < Data.TeamAliveVehicles.Length; i++)
            {
                Data.TeamAliveVehicles[i] = new();
            }

            if (Data.EnableRendering)
            {
                RenderSystem.Initialize(prefab, materials);
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime; // Not deterministic
            
            EnemyTargetSystem.Run();
            VehicleMovementSystem.Run(deltaTime);
            ShootSystem.Run(deltaTime);
            
            SpawnVehiclesSystem.Run();
            DieSystem.Run();

            if (Input.GetKeyUp(KeyCode.Space))
            {
                Data.EnableRendering = !Data.EnableRendering;
                if (Data.EnableRendering)
                {
                    RenderSystem.Initialize(prefab, materials);
                }
                else
                {
                    RenderSystem.Clear();
                }
            }

            if (Data.EnableRendering)
            {
                RenderSystem.Run();
            }
        }
    }
}