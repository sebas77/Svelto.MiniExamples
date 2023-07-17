// Copyright (c) Sean Nowotny

using Arch.Core;
using Arch.System;
using Logic.Arch.Components;

namespace Logic.Arch.Systems
{
    public class DieSystem : BaseSystem<World, float>
    {
        private readonly QueryDescription query;

        public DieSystem(World _world) : base(_world)
        {
            query = new QueryDescription().WithAll<HealthDC>();
        }

        public override void Update(in float _state)
        {
            World.Query(in query, (in Entity entity, ref HealthDC health) =>
            {
                if (health.Value <= 0)
                {
                    World.Destroy(entity);
                }
            });
        }
    }
}