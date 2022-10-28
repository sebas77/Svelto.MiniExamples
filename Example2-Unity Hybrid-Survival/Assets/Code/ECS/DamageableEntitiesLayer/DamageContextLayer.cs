using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.Damage
{
    public static class DamageContextLayer
    {
        public static DamageUnsortedEngines DamageLayerSetup(IEntityStreamConsumerFactory entityStreamConsumerFactory, EnginesRoot enginesRoot)
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
            return unsortedDamageEngines;
        }
    }
}