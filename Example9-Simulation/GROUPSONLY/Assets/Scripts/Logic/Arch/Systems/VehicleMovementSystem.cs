// Copyright (c) Sean Nowotny

using Arch.Core;
using Arch.System;
using Logic.Arch.Components;
using Unity.Mathematics;

namespace Logic.Arch.Systems
{
    public class VehicleMovementSystem : BaseSystem<World, float>
    {
        private readonly QueryDescription query;

        public VehicleMovementSystem(World _world) : base(_world)
        {
            query = new QueryDescription().WithAll<TargetDC, PositionDC>();
        }

        public override void Update(in float _deltaTime)
        {
            var deltaTime = _deltaTime;
            World.Query(in query, (ref TargetDC target, ref PositionDC position) =>
            {
                var targetEntity = target.Value;

                if (targetEntity == default)
                {
                    return;
                }

                var currentPosition = position.Value;
                var targetPosition = World.Get<PositionDC>(targetEntity).Value;

                if (math.distance(currentPosition, targetPosition) < Arch.Data.WeaponRange)
                {
                    return;
                }

                var direction = math.normalize(targetPosition - currentPosition);
                var newPosition = currentPosition + direction * Arch.Data.VehicleSpeed * deltaTime; // Not deterministic
                position.Value = newPosition;
            });
        }
    }
}