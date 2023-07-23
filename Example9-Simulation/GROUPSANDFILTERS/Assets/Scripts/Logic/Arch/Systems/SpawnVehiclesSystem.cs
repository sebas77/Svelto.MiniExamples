// Copyright (c) Sean Nowotny


using Arch.Core;
using Arch.System;
using Logic.Arch.Components;

namespace Logic.Arch.Systems
{
    public class SpawnVehiclesSystem: BaseSystem<World, float>
    {
        private readonly QueryDescription alive;

        public SpawnVehiclesSystem(World _world): base(_world)
        {
            alive = new QueryDescription().WithAll<TeamDC>();
        }

        public override void Update(in float state)
        {
            int aliveCount = World.CountEntities(in alive);
            
            int vacantSlots = Arch.Data.MaxVehicleCount - aliveCount;
            for (var i = 0; i < Arch.Data.MaxTeamCount; i++)
            {
                if (vacantSlots == 0)
                {
                    break;
                }

                Utils.SpawnVehicles(World, 1, i);
                vacantSlots--;
            }
        }
    }
}