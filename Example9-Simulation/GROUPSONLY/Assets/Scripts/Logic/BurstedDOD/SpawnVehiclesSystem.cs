// Copyright (c) Sean Nowotny

using Unity.Burst;

namespace Logic.BurstedDOD
{
    [BurstCompile]
    public static class SpawnVehiclesSystem
    {
        [BurstCompile]
        public static void Run(float deltaTime, ref Data data)
        {
            for (var i = 0; i < Data.MaxTeamCount; i++)
            {
                if (data.AliveCount == Data.MaxVehicleCount)
                {
                    break;
                }

                Utils.SpawnVehicles(1, i, ref data, deltaTime);
            }
        }
    }
}