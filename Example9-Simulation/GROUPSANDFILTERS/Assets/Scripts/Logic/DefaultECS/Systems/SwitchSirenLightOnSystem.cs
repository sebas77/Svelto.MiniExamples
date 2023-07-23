// Copyright (c) Sean Nowotny

using DefaultEcs;
using DefaultEcs.System;
using Logic.DefaultECS.Components;
using Unity.Mathematics;

namespace Logic.DefaultECS
{
    public class SwitchSirenLightOnSystem : AEntitySetSystem<float>
    {
        public SwitchSirenLightOnSystem(World _world) : base(
            _world.GetEntities().With<TargetDC>().With<HealthDC>().With<TimeUntilSirenSwitch>().Without<SirenLight>().AsSet(),
            false
        )
        {
        }

        protected override void Update(float _deltaTime, in Entity entity)
        {
            var health = entity.Get<HealthDC>().Value;
            var timeUntilSirenSwitch = entity.Get<TimeUntilSirenSwitch>().Value;

            if (timeUntilSirenSwitch <= 0)
            {
                entity.Set(
                    new SirenLight
                    {
                        LightIntensity = (int)math.min(150 - health, 100)
                    }
                );
                entity.Set(
                    new TimeUntilSirenSwitch
                    {
                        Value = health / 100
                    }
                );
            }
        }
    }
}