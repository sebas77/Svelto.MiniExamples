using Unity.Burst;
using Unity.Collections;

namespace Logic.BurstedDOD
{
    [BurstCompile]
    public static class DieSystem
    {
        [BurstCompile]
        public static void Run(ref Data data)
        {
            for (int i = 0; i < data.VehicleHealths.Length; i++)
            {
                if (!data.VehicleAliveStatuses[i])
                {
                    continue;
                }

                if (data.VehicleHealths[i] > 0)
                {
                    continue;
                }

                data.VehicleAliveStatuses[i] = false;
                var teamIndex = data.VehicleTeams[i];
                data.TeamAliveCounts[teamIndex]--;

                unsafe
                {
                    var teamLives = *Utils.TeamAliveNativeListFromIndex(teamIndex, ref data);
                    teamLives.RemoveAt(teamLives.IndexOf(i));
                }
                
                data.AliveCount--;
            }
        }
    }
}