// Copyright (c) Sean Nowotny

using Arch.Core;
using Logic.Arch.Components;
using UnityEngine;

namespace Logic.Arch.Systems
{
    public static class Utils
    {
        public static void SpawnVehicles(World world, int count, int teamIndex)
        {
            for (var i = 0; i < count; i++)
            {
                world.Create(
                    new PositionDC {Value = new(Random.Range(0, 100), Random.Range(0, 100))},
                    new TeamDC {Value = teamIndex},
                    new TargetDC {Value = Entity.Null},
                    new HealthDC {Value = 100}
                );
            }
        }
    }
}