// Copyright (c) Sean Nowotny

using Unity.Mathematics;

namespace Logic.DOD
{
    public class ShootSystem
    {
        public static void Run(float deltaTime)
        {
            for (var i = 0; i < Data.VehicleAliveStatuses.Length; i++)
            {
                if (!Data.VehicleAliveStatuses[i])
                {
                    continue;
                }

                var currentTarget = Data.VehicleTargets[i];
                if (currentTarget == -1)
                {
                    continue;
                }

                var currentPosition = Data.VehiclePositions[i];
                var targetPosition = Data.VehiclePositions[currentTarget];
                if (math.distance(currentPosition, targetPosition) <= Data.WeaponRange)
                {
                    Data.VehicleHealths[currentTarget] -= Data.WeaponDamage * deltaTime;
                }
            }
        }
    }
}