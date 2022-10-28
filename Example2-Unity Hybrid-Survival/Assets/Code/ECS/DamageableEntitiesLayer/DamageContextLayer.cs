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
            var damageSoundEngine = new DamageSoundEngine(entityStreamConsumerFactory);

            enginesRoot.AddEngine(applyDamageEngine);
            enginesRoot.AddEngine(deathEngine);
            enginesRoot.AddEngine(damageSoundEngine);

            var unsortedDamageEngines = new DamageUnsortedEngines(
                new FasterList<IStepEngine>(applyDamageEngine, damageSoundEngine, deathEngine));

            orderedEngines.Add(unsortedDamageEngines);
        }
    }
}