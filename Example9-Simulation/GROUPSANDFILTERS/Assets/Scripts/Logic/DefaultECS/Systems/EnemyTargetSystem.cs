// Copyright (c) Sean Nowotny

using System;
using DefaultEcs;
using DefaultEcs.System;
using Logic.DefaultECS;
using Logic.DefaultECS.Components;
using Random = UnityEngine.Random;

namespace Logic.DefaultECS
{
    public class EnemyTargetSystem : AEntitySetSystem<float>
    {
        private static bool[] enemyTeamAliveArr = new bool[Data.MaxTeamCount];
        private readonly EntityMultiMap<TeamDC> teamedMultiMap;

        public EnemyTargetSystem(World _world) : base(
            _world.GetEntities().With<TargetDC>().With<TeamDC>().AsSet(),
            false
        )
        {
            teamedMultiMap = _world.GetEntities().With<TeamDC>().AsMultiMap<TeamDC>();
        }

        protected override void Update(float state, in Entity entity)
        {
            if (entity.Get<TargetDC>().Value.IsAlive)
            {
                return;
            }
            
            var ownTeamIndex = entity.Get<TeamDC>();

            bool aliveEnemyTeamExists = false;
            for (var i = 0; i < Data.MaxTeamCount; i++)
            {
                if (ownTeamIndex.Value == i)
                {
                    enemyTeamAliveArr[i] = false; // Not an enemy team
                    continue;
                }

                if (teamedMultiMap.TryGetEntities(new TeamDC {Value = i}, out _))
                {
                    enemyTeamAliveArr[i] = true;
                    aliveEnemyTeamExists = true;
                }
                else
                {
                    enemyTeamAliveArr[i] = false;
                }
            }

            if (!aliveEnemyTeamExists)
            {
                return;
            }

            bool found;
            ReadOnlySpan<Entity> enemyTeamEntities;
            do
            {
                found = teamedMultiMap.TryGetEntities(
                    new TeamDC {Value = RandomEnemyTeamIndex(ownTeamIndex)},
                    out enemyTeamEntities
                );
            } while (!found);

            var enemyEntity = enemyTeamEntities[Random.Range(0, enemyTeamEntities.Length)];
            entity.Set(new TargetDC() {Value = enemyEntity});
        }

        private static int RandomEnemyTeamIndex(TeamDC team)
        {
            int randomTeamIndex;
            do
            {
                randomTeamIndex = Random.Range(0, Data.MaxTeamCount);
            } while (randomTeamIndex == team.Value || !enemyTeamAliveArr[randomTeamIndex]);

            return randomTeamIndex;
        }
    }
}