// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.DOD
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
                if (Data.VehicleAliveStatuses[i])
                {
                    continue;
                }

                Data.VehicleAliveStatuses[i] = true;
                Data.VehicleHealths[i] = 100;
                Data.VehiclePositions[i] = new(Random.Range(0, 100), Random.Range(0, 100));
                Data.VehicleTargets[i] = -1;
                Data.VehicleTeams[i] = teamIndex;
                Data.TeamAliveCounts[teamIndex]++;
                // Data.AliveTeamVehicleIndecies[teamIndex].Add(i);
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