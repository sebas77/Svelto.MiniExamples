using System.Collections;
using Svelto.Tasks;
using Svelto.Tasks.ExtraLean;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public class DieOfHungerDoofusesEngine : IQueryingEntitiesEngine
    {
        readonly IEntityFunctions _entityFunctions;
        public DieOfHungerDoofusesEngine(IEntityFunctions entityFunctions) { _entityFunctions = entityFunctions; }

        public void Ready() { ConsumingFood().RunOn(DoofusesStandardSchedulers.doofusesLogicScheduler); }

        public IEntitiesDB entitiesDB { private get; set; }

        IEnumerator ConsumingFood()
        {
            while (true)
            {
                var hungryDoofuses =
                    entitiesDB.QueryEntities<HungerEntityStruct>(GameGroups.DOOFUSES, out var doofusesCount);

                for (var j = 0; j < doofusesCount; j++)
                {
                    if (hungryDoofuses[j].hunger > 1000) ;
                    //         _entityFunctions.RemoveEntity<DoofusEntityDescriptor>(hungryDoofuses[j].ID);
                }

                yield return Yield.It;
            }
        }
    }
}