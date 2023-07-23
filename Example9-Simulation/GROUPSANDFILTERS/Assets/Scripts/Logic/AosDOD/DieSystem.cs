namespace Logic.AosDOD
{
    public class DieSystem
    {
        public static void Run()
        {
            for (int i = 0; i < Data.Vehicles.Length; i++)
            {
                ref var vehicle = ref Data.Vehicles[i];
                if (!vehicle.IsAlive)
                {
                    continue;
                }

                if (vehicle.Health > 0)
                {
                    continue;
                }

                vehicle.IsAlive = false;
                var teamIndex = vehicle.Team;
                Data.TeamAliveCounts[teamIndex]--;
                Data.TeamAliveVehicles[teamIndex].Remove(i);
                Data.AliveCount--;
            }
        }
    }
}