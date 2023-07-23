// Copyright (c) Sean Nowotny

using Unity.Burst;
using UnityEngine;

namespace Logic.BurstedDOD
{
    [BurstCompile]
    public static class EnemyTargetSystem
    {
        [BurstCompile]
        public static void Run(ref Data data)
        {
            bool moreThanOneTeamAlive = false;

            {
                int aliveTeams = 0;
                for (var k = 0; k < data.TeamAliveCounts.Length; k++)
                {
                    if (data.TeamAliveCounts[k] == 0)
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
                for (var i = 0; i < data.AliveCount; i++)
                {
                    data.VehicleTargets[i] = -1;
                }

                return;
            }

            for (var i = 0; i < data.AliveCount; i++)
            {
                var currentTargetIndex = data.VehicleTargets[i];
                if (currentTargetIndex != -1 && data.VehicleAliveStatuses[currentTargetIndex])
                {
                    continue;
                }

                var currentTeam = data.VehicleTeams[i];
                unsafe
                {
                    int enemyTeamIndex = 0;
                    do
                    {
                        enemyTeamIndex = Random.Range(0, Data.MaxTeamCount);
                    } while (
                        enemyTeamIndex == currentTeam ||
                        (*Utils.TeamAliveNativeListFromIndex(enemyTeamIndex, ref data)).Length == 0
                    );

                    var enemyTeamList = *Utils.TeamAliveNativeListFromIndex(enemyTeamIndex, ref data);
                    int targetIndex = enemyTeamList[Random.Range(0, enemyTeamList.Length)];
                    data.VehicleTargets[i] = targetIndex;
                }
            }
        }
    }
}