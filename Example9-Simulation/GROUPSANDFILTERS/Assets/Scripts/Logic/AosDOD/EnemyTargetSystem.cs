// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.AosDOD
{
    public class EnemyTargetSystem
    {
        public static void Run()
        {
            bool moreThanOneTeamAlive = false;

            {
                int aliveTeams = 0;
                for (var k = 0; k < Data.TeamAliveCounts.Length; k++)
                {
                    if (Data.TeamAliveCounts[k] == 0)
                    {
                        continue;
                    }

                    aliveTeams++;
                    if (aliveTeams > 1)
                    {
                        moreThanOneTeamAlive = true;
                        break;
                    }
                }
            }

            if (!moreThanOneTeamAlive)
            {
                for (var i = 0; i < Data.AliveCount; i++)
                {
                    Data.Vehicles[i].TargetIndex =  -1;
                }

                return;
            }

            for (var i = 0; i < Data.AliveCount; i++)
            {
                ref var vehicle = ref Data.Vehicles[i];
                var currentTargetIndex = vehicle.TargetIndex;
                if (currentTargetIndex != -1 && Data.Vehicles[currentTargetIndex].IsAlive)
                {
                    continue;
                }

                int enemyTeamIndex = 0;
                do
                {
                    enemyTeamIndex = Random.Range(0, Data.MaxTeamCount);
                } while (enemyTeamIndex == vehicle.Team || Data.TeamAliveVehicles[enemyTeamIndex].Count == 0);

                var enemyTeamList = Data.TeamAliveVehicles[enemyTeamIndex];
                int targetIndex = enemyTeamList[Random.Range(0, enemyTeamList.Count)];

                vehicle.TargetIndex = targetIndex;
            }
        }
    }
}