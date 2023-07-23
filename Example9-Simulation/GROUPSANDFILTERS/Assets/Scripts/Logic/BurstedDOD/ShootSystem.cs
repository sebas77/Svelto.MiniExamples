// Copyright (c) Sean Nowotny

using Unity.Mathematics;

namespace Logic.BurstedDOD
{
    public class ShootSystem
    {
        public static void Run(float deltaTime, ref Data data)
        {
            for (var i = 0; i < data.VehicleAliveStatuses.Length; i++)
            {
                if (!data.VehicleAliveStatuses[i])
                {
                    continue;
                }

                var currentTarget = data.VehicleTargets[i];
                if (currentTarget == -1)
                {
                    continue;
                }

                var currentPosition = data.VehiclePositions[i];
                var targetPosition = data.VehiclePositions[currentTarget];
                if (math.distance(currentPosition, targetPosition) <= Data.WeaponRange)
                {
                    data.VehicleHealths[currentTarget] -= Data.WeaponDamage * deltaTime;
                }
            }
        }
    }
}