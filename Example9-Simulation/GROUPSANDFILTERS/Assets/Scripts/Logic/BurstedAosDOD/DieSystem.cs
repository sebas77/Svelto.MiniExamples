using Unity.Burst;
using Unity.Collections;

namespace Logic.BurstedAosDOD
{
    [BurstCompile]
    public static class DieSystem
    {
        [BurstCompile]
        public static void Run(ref Data data)
        {
            for (int i = 0; i < data.Vehicles.Length; i++)
            {
                var vehicle = data.Vehicles[i];
                if (!vehicle.IsAlive)
                {
                    continue;
                }

                if (vehicle.Health > 0)
                {
                    continue;
                }

                data.Vehicles[i] = new Vehicle(vehicle) {IsAlive = false};
                
                unsafe
                {
                    var teamLives = *Utils.TeamAliveNativeListFromIndex(vehicle.Team, ref data);
                    teamLives.RemoveAt(teamLives.IndexOf(i));
                }
                
                data.TeamAliveCounts[vehicle.Team]--;
                data.AliveCount--;
            }
        }
    }
}