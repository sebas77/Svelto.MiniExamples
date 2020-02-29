using System.Collections;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;
using Svelto.Tasks.ExtraLean;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public class ConsumingFoodEngine : IQueryingEntitiesEngine
    {
        public ConsumingFoodEngine(IEntityFunctions entityFunctions) { _entityFunctions = entityFunctions; }

        public void Ready() { ConsumingFood().RunOn(DoofusesStandardSchedulers.foodScheduler); }

        public EntitiesDB entitiesDB { private get; set; }

        IEnumerator ConsumingFood()
        {
            ReusableWaitForSecondsEnumerator wait = new ReusableWaitForSecondsEnumerator(0.1f);
                
            while (true)
            {
                foreach (var group in GameGroups.FOOD.Groups)
                {
                    var mealStructs =
                        entitiesDB.QueryEntities<MealEntityStruct>(group);
                    
                    var foodcount = mealStructs.count;

                    for (var j = 0; j < foodcount; j++)
                    {
                        mealStructs[j].mealLeft -= mealStructs[j].eaters;
                        mealStructs[j].eaters   =  0;

                        if (mealStructs[j].mealLeft <= 0)
                            _entityFunctions.RemoveEntity<FoodEntityDescriptor>(mealStructs[j].ID);
                    }
                }

                while (wait.IsDone() == false) yield return Yield.It; 
            }
        }
        
        readonly IEntityFunctions _entityFunctions;
    }
}