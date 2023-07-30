using Svelto.DataStructures;
using Svelto.ECS;
using Unity.Mathematics;

namespace Logic.SveltoECS
{
    public class ShootSystem: IQueryingEntitiesEngine, IStepEngine<float>
    {
        public void Step(in float time)
        {
            foreach (var ((vehicles, positions, vehiclesCount), _) in entitiesDB.QueryEntities<TargetDC, PositionDC>(VehicleTag.Groups))
            {
                for (int i = 0; i < vehiclesCount; i++)
                {
                    ref var vehicle = ref vehicles[i];
                    var currentPosition = positions[i].Value;
                    var targetEntity = vehicle.target;
                    if (targetEntity.ToEGID(entitiesDB, out var targetEGID)) //if the target is still alive an EGID is returned
                    {
                        //todo: querying entities inside a loop like this is a killer for cache.
                        var targetPositions = entitiesDB.QueryEntitiesAndIndex<PositionDC>(targetEGID, out var index);
                        if (math.distance(currentPosition, targetPositions[index].Value) <= Data.WeaponRange)
                        {
                            (NB<HealthDC> targetHealths, _) = _mapped.Entities(targetEGID.groupID);

                            targetHealths[index].Value -= (Data.WeaponDamage * time);
                        }
                    }
                }
            }
        }

        public EntitiesDB entitiesDB { get; set; }

        public string name => nameof(ShootSystem);

        public void Ready()
        {
            _mapped = entitiesDB.QueryMappedEntities<HealthDC>(VehicleTag.Groups);
        }
        
        EGIDMultiMapper<HealthDC> _mapped;
    }
}