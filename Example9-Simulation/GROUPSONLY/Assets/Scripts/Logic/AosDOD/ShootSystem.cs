// Copyright (c) Sean Nowotny

using Unity.Mathematics;

namespace Logic.AosDOD
{
    public class ShootSystem
    {
        public static void Run(float deltaTime)
        {
            for (var i = 0; i < Data.Vehicles.Length; i++)
            {
                ref var vehicle = ref Data.Vehicles[i]; 
                if (!vehicle.IsAlive)
                {
                    continue;
                }

                if (vehicle.TargetIndex == -1)
                {
                    continue;
                }

                ref var target = ref Data.Vehicles[vehicle.TargetIndex];
                if (math.distance(vehicle.Position, target.Position) <= Data.WeaponRange)
                {
                    target.Health -= Data.WeaponDamage * deltaTime;
                }
            }
        }
    }
}