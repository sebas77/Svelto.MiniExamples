// Copyright (c) Sean Nowotny

using DefaultEcs;
using DefaultEcs.System;
using Logic.DefaultECS.Components;

namespace Logic.DefaultECS
{
    public class DieSystem : ISystem<float>
    {
        private readonly EntitySet entities;

        public bool IsEnabled { get; set; } = true;

        public DieSystem(World _world)
        {
            entities = _world.GetEntities().With<HealthDC>().AsSet();
        }

        public void Update(float state)
        {
            var nonce = entities.GetEntities();
            for (var i = 0; i < nonce.Length; i++)
            {
                var entity = nonce[i];
                if (entity.Get<HealthDC>().Value <= 0)
                {
                    entity.Dispose();
                    nonce = entities.GetEntities();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}