// Copyright (c) Sean Nowotny

using Svelto.ECS;

namespace Logic.SveltoECS
{
    public class DecrementTimersSystem: IQueryingEntitiesEngine, IStepEngine<float>
    {
        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }

        public void Step(in float time)
        {
            foreach (var ((times, vehicles), _) in entitiesDB.QueryEntities<TimeUntilSirenSwitch>(VehicleTag.Groups))
            {
                for (int i = 0; i < vehicles; i++)
                {
                    times[i].Value -= time;
                }
            }
        }

        public string name => nameof(DecrementTimersSystem);
    }
}