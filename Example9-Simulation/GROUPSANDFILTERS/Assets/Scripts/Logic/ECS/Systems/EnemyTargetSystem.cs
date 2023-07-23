// Copyright (c) Sean Nowotny

using Logic.ECS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Logic.ECS.Systems
{
    [UpdateInGroup(typeof(MySystemGroup))]
    public partial struct EnemyTargetSystem : ISystem
    {
        private EntityQuery query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            query = SystemAPI.QueryBuilder().WithAll<TeamDC>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var randomSingleton = SystemAPI.GetSingleton<SingletonRandom>();
            var teamCounts = new NativeArray<int>(Data.MaxTeamCount, state.WorldUpdateAllocator);
            foreach (var team in SystemAPI.Query<TeamDC>())
            {
                teamCounts[team.Value]++;
            }
            var entities = query.ToEntityArray(Allocator.Temp);

            foreach (var (target, ownTeam) in SystemAPI.Query<RefRW<TargetDC>, RefRO<TeamDC>>())
            {
                if (SystemAPI.Exists(target.ValueRO.Value))
                {
                    continue;
                }

                bool aliveEnemyTeamExists = false;
                for (var i = 0; i < DefaultECS.Data.MaxTeamCount; i++)
                {
                    if (i == ownTeam.ValueRO.Value)
                    {
                        continue;
                    }
                    else if (teamCounts[ownTeam.ValueRO.Value] > 0)
                    {
                        aliveEnemyTeamExists = true;
                        break;
                    }
                }

                if (!aliveEnemyTeamExists)
                {
                    continue;
                }

                Entity targetEntity;
                do
                {
                    targetEntity = entities[randomSingleton.Random.NextInt(0, entities.Length)];
                } while (SystemAPI.GetComponent<TeamDC>(targetEntity).Value == ownTeam.ValueRO.Value);

                target.ValueRW = new() {Value = targetEntity};
            }
        }
    }
}