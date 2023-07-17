// Copyright (c) Sean Nowotny

using UnityEngine;

namespace Logic.DOD
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
                    Data.VehicleTargets[i] = -1;
                }

                return;
            }

            for (var i = 0; i < Data.AliveCount; i++)
            {
                var currentTargetIndex = Data.VehicleTargets[i];
                if (currentTargetIndex != -1 && Data.VehicleAliveStatuses[currentTargetIndex])
                {
                    continue;
                }

                var currentTeam = Data.VehicleTeams[i];
                int enemyTeamIndex = 0;
                do
                {
                    enemyTeamIndex = Random.Range(0, Data.MaxTeamCount);
                } while (enemyTeamIndex == currentTeam || Data.TeamAliveVehicles[enemyTeamIndex].Count == 0);

                var enemyTeamList = Data.TeamAliveVehicles[enemyTeamIndex];
                int targetIndex = enemyTeamList[Random.Range(0, enemyTeamList.Count)];

                Data.VehicleTargets[i] = targetIndex;
            }
        }
    }
}