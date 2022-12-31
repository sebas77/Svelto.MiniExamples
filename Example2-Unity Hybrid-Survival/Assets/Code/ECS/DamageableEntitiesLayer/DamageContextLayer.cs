using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive.Damage
{
    public static class DamageLayerContext
    {
        public static void Setup(EnginesRoot enginesRoot, FasterList<IStepEngine> orderedEngines)
        {
            //damage engines
            var applyDamageEngine = new ApplyDamageToDamageableEntitiesEngine();

            enginesRoot.AddEngine(applyDamageEngine);

            var unsortedDamageEngines = new DamageUnsortedEngines(new FasterList<IStepEngine>(applyDamageEngine));

            orderedEngines.Add(unsortedDamageEngines);
        }
    }
}