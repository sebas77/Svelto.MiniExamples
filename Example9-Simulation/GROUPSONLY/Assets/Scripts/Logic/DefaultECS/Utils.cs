// Copyright (c) Sean Nowotny

using DefaultEcs;
using Logic.DefaultECS.Components;
using UnityEngine;

namespace Logic.DefaultECS
{
    public static class Utils
    {
        public static void SpawnVehicles(World world, int count, int teamIndex)
        {
            for (var i = 0; i < count; i++)
            {
                var vehicle = world.CreateEntity();
                vehicle.Set(new PositionDC {Value = new(Random.Range(0, 100), Random.Range(0, 100))});
                vehicle.Set(new TeamDC {Value = teamIndex});
                vehicle.Set(new TargetDC {Value = default});
                vehicle.Set(new HealthDC {Value = 100});
                vehicle.Set<TimeUntilSirenSwitch>();
            }
        }
    }
}