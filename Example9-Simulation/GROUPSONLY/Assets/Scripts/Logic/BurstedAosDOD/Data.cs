using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Logic.BurstedAosDOD
{
    public struct Vehicle
    {
        public Vehicle(Vehicle vehicle)
        {
            Position = vehicle.Position;
            Health = vehicle.Health;
            Team = vehicle.Team;
            TargetIndex = vehicle.TargetIndex;
            IsAlive = vehicle.IsAlive;
        }
        
        public float2 Position;
        public float Health;
        public int Team;
        public int TargetIndex;
        public bool IsAlive;
    }
    
    [BurstCompile]
    public struct Data : IDisposable
    {
        public static readonly int MaxVehicleCount = 1000;
        public static readonly int MaxTeamCount = 4;
        public static readonly float WeaponRange = 5;
        public static readonly float WeaponDamage = 100;
        public static readonly float VehicleSpeed = 5;

        [MarshalAs(UnmanagedType.U1)] public bool EnableRendering;
        public int AliveCount;

        public NativeArray<Vehicle> Vehicles;

        public NativeArray<int> TeamAliveCounts;

        public NativeList<int> Team0AliveVehicles;
        public NativeList<int> Team1AliveVehicles;
        public NativeList<int> Team2AliveVehicles;
        public NativeList<int> Team3AliveVehicles;

        public Data(bool enabledRendering = true)
        {
            Team3AliveVehicles = new NativeList<int>(MaxVehicleCount, Allocator.Persistent);
            EnableRendering = enabledRendering;
            AliveCount = 0;
            Vehicles = new NativeArray<Vehicle>(MaxVehicleCount, Allocator.Persistent);
            TeamAliveCounts = new NativeArray<int>(MaxTeamCount, Allocator.Persistent);
            Team0AliveVehicles = new NativeList<int>(MaxVehicleCount, Allocator.Persistent);
            Team1AliveVehicles = new NativeList<int>(MaxVehicleCount, Allocator.Persistent);
            Team2AliveVehicles = new NativeList<int>(MaxVehicleCount, Allocator.Persistent);
        }

        public void Dispose()
        {
            Vehicles.Dispose();
            TeamAliveCounts.Dispose();
            Team0AliveVehicles.Dispose();
            Team1AliveVehicles.Dispose();
            Team2AliveVehicles.Dispose();
            Team3AliveVehicles.Dispose();
        }
    }
}