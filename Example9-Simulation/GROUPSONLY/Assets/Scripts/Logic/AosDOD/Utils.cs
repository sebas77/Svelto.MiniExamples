// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.AosDOD
{
    public static class Utils
    {
        public static void SpawnVehicles(int count, int teamIndex)
        {
            if (count == 0)
            {
                return;
            }

            int spawned = 0;
            for (var i = 0; i < Data.MaxVehicleCount; i++)
            {
                if (Data.Vehicles[i].IsAlive)
                {
                    continue;
                }

                Data.Vehicles[i] = new Vehicle
                {
                    IsAlive = true, Health = 100, Position = new(Random.Range(0, 100), Random.Range(0, 100)),
                    TargetIndex = -1, Team = teamIndex
                };
                Data.TeamAliveCounts[teamIndex]++;
                Data.TeamAliveVehicles[teamIndex].Add(i);
                Data.AliveCount++;

                spawned++;
                if (spawned == count)
                {
                    break;
                }
            }
        }
    }
}