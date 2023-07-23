namespace Logic.DOD
{
    public class DieSystem
    {
        public static void Run()
        {
            for (int i = 0; i < Data.VehicleHealths.Length; i++)
            {
                if (!Data.VehicleAliveStatuses[i])
                {
                    continue;
                }

                if (Data.VehicleHealths[i] > 0)
                {
                    continue;
                }

                Data.VehicleAliveStatuses[i] = false;
                var teamIndex = Data.VehicleTeams[i];
                Data.TeamAliveCounts[teamIndex]--;
                // Data.AliveTeamVehicleIndecies[teamIndex].Remove(i);
                Data.TeamAliveVehicles[teamIndex].Remove(i);
                Data.AliveCount--;
            }
        }
    }
}