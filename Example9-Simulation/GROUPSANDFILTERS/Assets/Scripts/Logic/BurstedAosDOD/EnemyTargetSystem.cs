// Copyright (c) Sean Nowotny

using Unity.Burst;
using UnityEngine;

namespace Logic.BurstedAosDOD
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
                    data.Vehicles[i] = new Vehicle(data.Vehicles[i]) {TargetIndex = -1};
                }

                return;
            }

            for (var i = 0; i < data.AliveCount; i++)
            {
                var currentTargetIndex = data.Vehicles[i].TargetIndex;
                if (currentTargetIndex != -1 && data.Vehicles[currentTargetIndex].IsAlive)
                {
                    continue;
                }

                var currentTeam = data.Vehicles[i].Team;
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
                    data.Vehicles[i] = new Vehicle(data.Vehicles[i]) {TargetIndex = targetIndex};
                }
            }
        }
    }
}