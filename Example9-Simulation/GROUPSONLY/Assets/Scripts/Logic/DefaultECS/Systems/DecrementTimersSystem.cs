// Copyright (c) Sean Nowotny

using DefaultEcs;
using DefaultEcs.System;
using Logic.DefaultECS.Components;
using Unity.Mathematics;

namespace Logic.DefaultECS
{
    public class DecrementTimersSystem : AEntitySetSystem<float>
    {
        public DecrementTimersSystem(World _world) : base(
            _world.GetEntities().With<TimeUntilSirenSwitch>().AsSet(),
            false
        )
        {
        }

        protected override void Update(float _deltaTime, in Entity entity)
        {
            entity.Set(new TimeUntilSirenSwitch
                {
                    Value = entity.Get<TimeUntilSirenSwitch>().Value - _deltaTime
                }
            );
        }
    }
}