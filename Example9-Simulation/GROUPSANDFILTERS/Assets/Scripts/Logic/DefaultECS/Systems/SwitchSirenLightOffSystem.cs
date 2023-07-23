// Copyright (c) Sean Nowotny

using DefaultEcs;
using DefaultEcs.System;
using Logic.DefaultECS.Components;

namespace Logic.DefaultECS
{
    public class SwitchSirenLightOffSystem : AEntitySetSystem<float>
    {
        public SwitchSirenLightOffSystem(World _world) : base(
            _world.GetEntities().With<TargetDC>().With<TimeUntilSirenSwitch>().With<SirenLight>().AsSet(),
            false
        )
        {
        }

        protected override void Update(float _deltaTime, in Entity entity)
        {
            var timeUntilSirenSwitch = entity.Get<TimeUntilSirenSwitch>().Value;

            if (timeUntilSirenSwitch <= 0)
            {
                entity.Remove<SirenLight>();
                entity.Set(
                    new TimeUntilSirenSwitch
                    {
                        Value = 0.1f
                    }
                );
            }
        }
    }
}