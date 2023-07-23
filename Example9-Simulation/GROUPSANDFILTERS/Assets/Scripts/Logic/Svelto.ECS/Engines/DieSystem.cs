using Svelto.ECS;

namespace Logic.SveltoECS
{
    public class DieSystem: IQueryingEntitiesEngine, IStepEngine<float>
    {
        public DieSystem(IEntityFunctions functions)
        {
            _functions = functions;
        }

        public void Step(in float time)
        {
            foreach (var ((healths, ids, aliveCount), group) in entitiesDB.QueryEntities<HealthDC>(VehicleTag.Groups))
            {
                for (int i = 0; i < aliveCount; ++i)
                {
                    if (healths[i].Value <= 0)
                        _functions.RemoveEntity<VehicleDescriptor>(ids[i], group); //will be removed from persistent filters too
                }
            }
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        
        public string name => nameof(DieSystem);
        
        readonly IEntityFunctions _functions;
    }
}