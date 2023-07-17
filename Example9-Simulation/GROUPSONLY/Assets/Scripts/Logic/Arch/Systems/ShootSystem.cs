// Copyright (c) Sean Nowotny

using Arch.Core;
using Arch.System;
using Logic.Arch.Components;
using Unity.Mathematics;

namespace Logic.Arch.Systems
{
    public class ShootSystem : BaseSystem<World, float>
    {
        private readonly QueryDescription query;

        public ShootSystem(World _world) : base(_world)
        {
            query = new QueryDescription().WithAll<TargetDC, PositionDC>();
        }

        public override void Update(in float _deltaTime)
        {
            var deltaTime = _deltaTime;
            World.Query(in query, (ref TargetDC target, ref PositionDC position) =>
            {
                var currentPosition = position.Value;
                var targetEntity = target.Value;
                var targetPosition = World.Get<PositionDC>(targetEntity).Value;

                if (math.distance(currentPosition, targetPosition) <= Arch.Data.WeaponRange)
                {
                    var targetHealth = World.Get<HealthDC>(targetEntity).Value;
                    World.Set(
                        targetEntity,
                        new HealthDC
                        {
                            Value = targetHealth - Arch.Data.WeaponDamage * deltaTime // Not deterministic
                        }
                    );
                }
            });
        }
    }
}