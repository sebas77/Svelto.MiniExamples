// Copyright (c) Sean Nowotny

using DefaultEcs;
using DefaultEcs.System;
using UnityEngine;

namespace Logic.DefaultECS
{
    public class DefaultEcsGameHandler : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Material[] materials;
        [SerializeField] private Material sirenLightMaterial;

        private World world;
        private SequentialSystem<float> sequentialSystem;
        private RenderSystem renderSystem;

        private void Start()
        {
            world = new World();

            sequentialSystem = new SequentialSystem<float>(
                new SpawnVehiclesSystem(world),
                new EnemyTargetSystem(world),
                new VehicleMovementSystem(world),
                new SwitchSirenLightOnSystem(world),
                new SwitchSirenLightOffSystem(world),
                new DecrementTimersSystem(world),
                new ShootSystem(world),
                new DieSystem(world)
            );

            renderSystem = new RenderSystem(world, prefab, materials, sirenLightMaterial);
            world.Subscribe(this);
        }

        private void Update()
        {
            sequentialSystem.Update(Time.deltaTime);

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
    }
}