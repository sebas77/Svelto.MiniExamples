using System;
using Svelto.ECS;
using UnityEngine;

namespace Logic.SveltoECS
{
    public class EnemyTargetSystem: IQueryingEntitiesEngine, IStepEngine<float>
    {
        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public void Step(in float time)
        {
            foreach (var ((vehicles, count), group) in entitiesDB.QueryEntities<TargetDC>(VehicleTag.Groups))
            {
                for (var i = 0; i < count; ++i)
                {
                    ref var vehicle = ref vehicles[i];
                    if (vehicle.target.Exists(entitiesDB)) //the current vehicle has a target. Todo: this is not ECS oriented, we should have a sub-set of vehicles without target instead
                        continue;

                    //pick up a random team index to not always attack the same team
                    var enemyTeamIndex = UnityEngine.Random.Range(0, Data.MaxTeamCount - 1);
                    var teamGroup = VehicleTag.BuildGroup;
                    int offset = 0;

                    while (offset++ < Data.MaxTeamCount) //search for target to attack in a team different than the entity one
                    {
                        var wrappedIndex = (enemyTeamIndex + offset) % Data.MaxTeamCount; //never choose the vehicle team
                        var enemyTeam = teamGroup + (uint)wrappedIndex;
                        if (enemyTeam == group) continue;
                        //todo: jumping groups like this is a killer for the cache, it would be wiser to have a better strategy to pick up enemies to minimise the number of queries
                        var (_, entityIDs, enemyTeamCount) = entitiesDB.QueryEntities<PositionDC>(enemyTeam);
                        if (enemyTeamCount > 0)
                        { //get any random entity from a team with still alive vehicles
                            uint index = (uint)UnityEngine.Random.Range(0, enemyTeamCount);
                            var egid = new EGID(entityIDs[index], enemyTeam);
                            vehicle.target = entitiesDB.GetEntityReference(egid);
                            
                            break;
                        }

                        enemyTeamIndex++;
                    }
                }
            }
        }

        public string name => nameof(EnemyTargetSystem);
    }
}