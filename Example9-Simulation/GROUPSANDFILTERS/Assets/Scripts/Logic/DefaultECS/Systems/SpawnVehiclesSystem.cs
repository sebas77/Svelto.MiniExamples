// Copyright (c) Sean Nowotny

using DefaultEcs;
using DefaultEcs.System;
using Logic.DefaultECS;
using Logic.DefaultECS.Components;

namespace Logic.DefaultECS
{
    public class SpawnVehiclesSystem : ISystem<float>
    {
        private readonly World world;
        private readonly EntitySet alive;

        public SpawnVehiclesSystem(World _world)
        {
            world = _world;
            alive = world.GetEntities().With<TeamDC>().AsSet();
        }

        public bool IsEnabled { get; set; } = true;

        public void Update(float state)
        {
            var aliveCount = alive.GetEntities().Length;
            int vacantSlots = Data.MaxVehicleCount - aliveCount;
            for (var i = 0; i < Data.MaxTeamCount; i++)
            {
                if (vacantSlots == 0)
                {
                    break;
                }

                Utils.SpawnVehicles(world, 1, i);
                vacantSlots--;
            }
        }

        public void Dispose()
        {
        }
    }
}