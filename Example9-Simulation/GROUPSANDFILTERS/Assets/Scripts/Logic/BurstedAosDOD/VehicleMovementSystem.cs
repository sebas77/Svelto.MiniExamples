// Copyright (c) Sean Nowotny

using Unity.Mathematics;

namespace Logic.BurstedAosDOD
{
    public class VehicleMovementSystem
    {
        public static void Run(float deltaTime, ref Data data)
        {
            for (var i = 0; i < data.Vehicles.Length; i++)
            {
                if (!data.Vehicles[i].IsAlive)
                {
                    continue;
                }

                var currentTargetIndex = data.Vehicles[i].TargetIndex;
                if (currentTargetIndex == -1)
                {
                    continue;
                }

                var currentPosition = data.Vehicles[i].Position;
                var targetPosition = data.Vehicles[currentTargetIndex].Position;

                if (math.distance(currentPosition, targetPosition) < Data.WeaponRange)
                {
                    continue;
                }

                var direction = math.normalize(targetPosition - currentPosition);
                var newPosition = currentPosition + direction * Data.VehicleSpeed * deltaTime;

                var vehicle = new Vehicle(data.Vehicles[i]) {Position = newPosition};
                data.Vehicles[i] = vehicle;
            }
        }
    }
}