// Copyright (c) Sean Nowotny

using Arch.Core;
using Arch.System;
using UnityEngine;
using Logic.Arch.Systems;
using Unity.VisualScripting;

namespace Logic.Arch
{
    public class ArchGameHandler : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Material[] materials;

        private World world;
        private Group<float> systems;
        private RenderSystem renderSystem;

        private void Start()
        {
            world = World.Create();

            systems = new Group<float>(
                new SpawnVehiclesSystem(world),
                new EnemyTargetSystem(world),
                new VehicleMovementSystem(world),
                new ShootSystem(world),
                new DieSystem(world)
            );
            systems.Initialize();

            renderSystem = new RenderSystem(world, prefab, materials);
            renderSystem.Initialize();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            systems.BeforeUpdate(deltaTime);
            systems.Update(deltaTime);
            systems.AfterUpdate(deltaTime);

            if (Input.GetKeyUp(KeyCode.Space))
            {
                Data.EnableRendering = !Data.EnableRendering;
                if (Data.EnableRendering)
                {
                    renderSystem.Initialize();
                }
                else
                {
                    renderSystem.Clear();
                }
            }

            if (Data.EnableRendering)
            {
                renderSystem.Update(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            systems.Dispose();
            renderSystem.Dispose();
        }
    }
}