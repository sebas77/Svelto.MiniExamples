using System.Collections;

namespace Svelto.ECS.Example.Survive.Characters
{
    public class CharactersDeathEngine:IQueryingEntitiesEngine
    {
        public void Ready()
        {
            CheckEnergy().Run();
        }

        IEnumerator CheckEnergy()
        {
            while (true)
            {
                entitiesDB.ExecuteOnAllEntities(ECSGroups.DamageableGroups,
                                                (ref HealthEntityStruct health, IEntitiesDB entitiesdb, int index) =>
                        {
                            if (health.currentHealth <= 0)
                                health.dead = true;
                        });

                yield return null;
            }
        }

        public IEntitiesDB entitiesDB { set; private get; }
    }
}