using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.Damage
{
    public static class DamageContextLayer
    {
        public static void DamageLayerSetup(IEntityStreamConsumerFactory entityStreamConsumerFactory,
            EnginesRoot enginesRoot, FasterList<IStepEngine> orderedEngines)
        {
            //damage engines
            var applyDamageEngine = new ApplyDamageToDamageableEntitiesEngine(entityStreamConsumerFactory);
            var deathEngine = new DispatchKilledEntitiesEngine();

            enginesRoot.AddEngine(applyDamageEngine);
            enginesRoot.AddEngine(deathEngine);

            var unsortedDamageEngines = new DamageUnsortedEngines(
                new FasterList<IStepEngine>(applyDamageEngine, deathEngine));

            orderedEngines.Add(unsortedDamageEngines);
        }
    }
}