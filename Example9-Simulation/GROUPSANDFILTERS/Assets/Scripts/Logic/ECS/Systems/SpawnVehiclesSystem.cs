// Copyright (c) Sean Nowotny

using Logic.ECS.Components;
using Unity.Entities;
using Random = UnityEngine.Random;

namespace Logic.ECS.Systems
{
    [UpdateBefore(typeof(DieSystem))]
    [UpdateBefore(typeof(ShootSystem))]
    [UpdateInGroup(typeof(MySystemGroup))]
    public partial class SpawnVehiclesSystem : SystemBase
    {
        private EntityQuery query;

        protected override void OnCreate()
        {
            query = GetEntityQuery(typeof(TeamDC));
        }

        protected override void OnUpdate()
        {
            var aliveCount = query.CalculateEntityCount();
            int vacantSlots = Data.MaxVehicleCount - aliveCount;
            for (var i = 0; i < Data.MaxTeamCount; i++)
            {
                if (vacantSlots == 0)
                {
                    break;
                }

                var vehicle = EntityManager.CreateEntity();

                EntityManager.AddComponentData(
                    vehicle,
                    new PositionDC
                    {
                        Value = new(Random.Range(0, 100), Random.Range(0, 100))
                    }
                );

                EntityManager.AddComponentData(
                    vehicle,
                    new TeamDC
                    {
                        Value = i
                    }
                );

                EntityManager.AddComponent<TargetDC>(vehicle);

                EntityManager.AddComponentData(vehicle,
                    new HealthDC
                    {
                        Value = 100
                    }
                );

                vacantSlots--;
            }
        }
    }
}