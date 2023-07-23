// Copyright (c) Sean Nowotny

using Unity.Mathematics;

namespace Logic.BurstedDOD
{
    public class VehicleMovementSystem
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

                if (math.distance(currentPosition, targetPosition) < Data.WeaponRange)
                {
                    continue;
                }

                var direction = math.normalize(targetPosition - currentPosition);
                var newPosition = currentPosition + direction * Data.VehicleSpeed * deltaTime;
                data.VehiclePositions[i] = newPosition;
            }
        }
    }
}