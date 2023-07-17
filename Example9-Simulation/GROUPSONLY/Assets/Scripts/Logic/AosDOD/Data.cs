using System.Collections.Generic;
using Unity.Mathematics;

namespace Logic.AosDOD
{
    public struct Vehicle
    {
        public float2 Position;
        public float Health;
        public int Team;
        public int TargetIndex;
        public bool IsAlive;
    }
    
    public static class Data
    {
        public static bool EnableRendering = true;

        public static readonly int MaxVehicleCount = 1000;
        public static readonly int MaxTeamCount = 4;
        
        public static Vehicle[] Vehicles = new Vehicle[MaxVehicleCount];
        
        public static int[] TeamAliveCounts = new int[MaxTeamCount];
        public static List<int>[] TeamAliveVehicles = new List<int>[MaxTeamCount];
        
        public static int AliveCount = 0;
        public static readonly float WeaponRange = 5;
        public static readonly float WeaponDamage = 100;
        public static readonly float VehicleSpeed = 5;
    }
}