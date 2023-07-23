// Copyright (c) Sean Nowotny

using DefaultEcs;
using DefaultEcs.System;
using Logic.DefaultECS;
using Logic.DefaultECS.Components;
using Unity.Mathematics;

namespace Logic.DefaultECS
{
    public class ShootSystem : AEntitySetSystem<float>
    {
        public ShootSystem(World _world) : base(
            _world.GetEntities().With<TargetDC>().With<PositionDC>().AsSet(),
            false
        )
        {
        }

        protected override void Update(float _deltaTime, in Entity entity)
        {
            var currentPosition = entity.Get<PositionDC>().Value;
            var targetEntity = entity.Get<TargetDC>().Value;
            if (!targetEntity.IsAlive)
            {
                return;
            }
            var targetPosition = targetEntity.Get<PositionDC>().Value;
            if (math.distance(currentPosition, targetPosition) <= Data.WeaponRange)
            {
                var targetHealth = targetEntity.Get<HealthDC>().Value;
                targetEntity.Set(new HealthDC
                    {
                        Value = targetHealth - Data.WeaponDamage * _deltaTime
                    }
                );
            }
        }
    }
}