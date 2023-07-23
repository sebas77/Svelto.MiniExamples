// Copyright (c) Sean Nowotny

using DefaultEcs;
using DefaultEcs.System;
using Logic.DefaultECS;
using Logic.DefaultECS.Components;
using Unity.Mathematics;

namespace Logic.DefaultECS
{
    public class VehicleMovementSystem : AEntitySetSystem<float>
    {
        public VehicleMovementSystem(World _world) : base(
            _world.GetEntities().With<TargetDC>().With<PositionDC>().AsSet(),
            false
        )
        {
        }

        protected override void Update(float _deltaTime, in Entity entity)
        {
            var targetEntity = entity.Get<TargetDC>().Value;
            if (targetEntity == default)
            {
                return;
            }
            
            var currentPosition = entity.Get<PositionDC>().Value;
            var targetPosition = targetEntity.Get<PositionDC>().Value;
            
            if (math.distance(currentPosition, targetPosition) < Data.WeaponRange)
            {
                return;
            }
                
            var direction = math.normalize(targetPosition - currentPosition);
            var newPosition = currentPosition + direction * Data.VehicleSpeed * (float)_deltaTime; // Not deterministic
            entity.Set(new PositionDC {Value = newPosition});
        }
    }
}