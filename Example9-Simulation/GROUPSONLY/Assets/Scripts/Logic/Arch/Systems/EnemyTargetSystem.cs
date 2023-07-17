// Copyright (c) Sean Nowotny

using Arch.Core;
using Arch.System;
using Logic.Arch.Components;
using Random = UnityEngine.Random;

namespace Logic.Arch.Systems
{
    public class EnemyTargetSystem : BaseSystem<World, float>
    {
        private readonly QueryDescription query;

        public EnemyTargetSystem(World _world) : base(_world)
        {
            query = new QueryDescription().WithAll<TargetDC>();
        }

        public override void Update(in float state)
        {
            var entities = new Entity[World.CountEntities(query)];
            World.GetEntities(query, entities);
            
            World.Query(in query, (ref PositionDC position, ref TargetDC target, ref TeamDC ownTeamIndex) =>
            {
                if (World.IsAlive(target.Value))
                {
                    return;
                }

                // TODO: Think of something to make the logic equivalent to the other solutions without bad performance

                target.Value = entities[Random.Range(0, entities.Length)];
            });
        }
    }
}