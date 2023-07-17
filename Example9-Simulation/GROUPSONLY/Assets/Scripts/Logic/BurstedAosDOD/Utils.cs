// Copyright (c) Sean Nowotny

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Unity.Mathematics;

namespace Logic.BurstedAosDOD
{
    [BurstCompile]
    public static class Utils
    {
        [BurstCompile]
        public static void SpawnVehicles(int count, int teamIndex, ref Data data, float deltaTime)
        {
            if (count == 0)
            {
                return;
            }

            int spawned = 0;
            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint) (deltaTime * 100000));

            for (var i = 0; i < Data.MaxVehicleCount; i++)
            {
                if (data.Vehicles[i].IsAlive)
                {
                    continue;
                }

                Vehicle vehicle = new Vehicle
                {
                    IsAlive = true,
                    Health = 100,
                    Position = new float2(random.NextFloat(0, 100), random.NextFloat(0, 100)),
                    TargetIndex = -1,
                    Team = teamIndex
                };

                data.Vehicles[i] = vehicle;
                data.TeamAliveCounts[teamIndex]++;

                unsafe
                {
                    var teamAlives = *TeamAliveNativeListFromIndex(teamIndex, ref data);
                    teamAlives.Add(i);
                }

                data.AliveCount++;

                spawned++;
                if (spawned == count)
                {
                    break;
                }
            }
        }

        [BurstCompile]
        public static unsafe NativeList<int>* TeamAliveNativeListFromIndex(int index, ref Data data)
        {
            switch (index)
            {
                case 0:
                {
                    return (NativeList<int>*) UnsafeUtility.AddressOf(ref data.Team0AliveVehicles);
                }
                case 1:
                {
                    return (NativeList<int>*) UnsafeUtility.AddressOf(ref data.Team1AliveVehicles);
                }
                case 2:
                {
                    return (NativeList<int>*) UnsafeUtility.AddressOf(ref data.Team2AliveVehicles);
                }
                case 3:
                {
                    return (NativeList<int>*) UnsafeUtility.AddressOf(ref data.Team3AliveVehicles);
                }
                default:
                {
                    Debug.LogError("Invalid Index");
                    return (NativeList<int>*) UnsafeUtility.AddressOf(ref data.Team3AliveVehicles);
                }
            }
        }
    }
}