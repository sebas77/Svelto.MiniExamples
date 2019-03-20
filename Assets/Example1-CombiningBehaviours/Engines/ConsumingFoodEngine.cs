using System.Collections;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;
using Svelto.Tasks.ExtraLean;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class ConsumingFoodEngine : IQueryingEntitiesEngine
    {
        readonly IEntityFunctions _entityFunctions;
        public ConsumingFoodEngine(IEntityFunctions entityFunctions) { _entityFunctions = entityFunctions; }

        public void Ready() { ConsumingFood().RunOn(DoofusesStandardSchedulers.foodScheduler); }

        public IEntitiesDB entitiesDB { private get; set; }

        IEnumerator ConsumingFood()
        {
            ReusableWaitForSecondsEnumerator wait = new ReusableWaitForSecondsEnumerator(0.1f);
                
            while (true)
            {
                var mealStructs =
                    entitiesDB.QueryEntities<MealEntityStruct>(GameGroups.FOOD, out var foodcount);

                for (var j = 0; j < foodcount; j++)
                {
                    mealStructs[j].mealLeft -= mealStructs[j].eaters;
                    
                    mealStructs[j].eaters = 0;

                    if (mealStructs[j].mealLeft <= 0)
                        _entityFunctions.RemoveEntity<FoodEntityDescriptor>(mealStructs[j].ID);
                }
                
                while (wait.IsDone()) yield return Yield.It; 
            }
        }
    }
}