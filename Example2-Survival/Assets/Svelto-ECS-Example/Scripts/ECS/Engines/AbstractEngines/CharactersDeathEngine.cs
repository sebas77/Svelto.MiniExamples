using System.Collections;

namespace Svelto.ECS.Example.Survive.Characters
{
    public class CharactersDeathEngine : IQueryingEntitiesEngine
    {
        public void Ready() { CheckEnergy().Run(); }

        public IEntitiesDB entitiesDB { set; private get; }

        IEnumerator CheckEnergy()
        {
            void EnumeratorHelper()
            {
                var healths = entitiesDB.QueryEntities<HealthEntityStruct>(ECSGroups.DamageableGroups);

                foreach (ref var health in healths)
                    if (health.currentHealth <= 0)
                        health.dead = true;
            }

            while (true)
            {
                EnumeratorHelper();

                yield return null;
            }
        }
    }
}