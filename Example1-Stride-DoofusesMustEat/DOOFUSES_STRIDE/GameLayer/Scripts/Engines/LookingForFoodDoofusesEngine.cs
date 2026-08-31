using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    [Sequenced(nameof(DoofusesEngineNames.LookingForFoodDoofusesEngine))]
    public class LookingForFoodDoofusesEngine: IQueryingEntitiesEngine, IUpdateEngine
    {
        public void Ready() { }

        public LookingForFoodDoofusesEngine(IEntityFunctions functions)
        {
            _functions = functions;
        }

        public string name => nameof(LookingForFoodDoofusesEngine);

        public void Step(in float _param)
        {
            //Iterate NOEATING RED doofuses to look for RED food and MOVE them to EATING state if food is found
            CreateJobForDoofusesAndFood(
                GameGroups.RED_FOOD_NOT_EATEN.Groups
              , GameGroups.RED_DOOFUSES_NOT_EATING.Groups
              , GameGroups.RED_DOOFUSES_EATING.BuildGroup
              , GameGroups.RED_FOOD_EATEN.BuildGroup);

            //Iterate NOEATING BLUE doofuses to look for BLUE food and MOVE them to EATING state if food is found
            CreateJobForDoofusesAndFood(
                GameGroups.BLUE_FOOD_NOT_EATEN.Groups
              , GameGroups.BLUE_DOOFUSES_NOT_EATING.Groups
              , GameGroups.BLUE_DOOFUSES_EATING.BuildGroup
              , GameGroups.BLUE_FOOD_EATEN.BuildGroup);
        }

        /// <summary>
        /// All the available doofuses will start to hunt for available food
        /// </summary>
        void CreateJobForDoofusesAndFood(FasterReadOnlyList<ExclusiveGroupStruct> groupsWithAvailableFood
          , FasterReadOnlyList<ExclusiveGroupStruct> groupsWithAvailableDoofuses, ExclusiveBuildGroup eatingDoofusesGroup
          , ExclusiveBuildGroup eatenFoodGroup)
        {
            //query all the available food
            var availableFoodComponents = entitiesDB.QueryEntities<PositionComponent>(groupsWithAvailableFood).GetEnumerator();
            //query all the doofuses that are not eating
            var availableDoofusesComponents = entitiesDB.QueryEntities<MealInfoComponent>(groupsWithAvailableDoofuses).GetEnumerator();

            while (availableFoodComponents.MoveNext() && availableDoofusesComponents.MoveNext())
            {
                ((_, NativeEntityIDs foodIDs, int availableFoodCount), ExclusiveGroupStruct currentFoodGroup) = availableFoodComponents.Current;
                var ((doofusesEntities, doofusesIDs, doofusesCount), currentDoofusesGroup) = availableDoofusesComponents.Current;
                var eatingDoofuses = MathF.Min(availableFoodCount, doofusesCount);

                if (eatingDoofuses > 0)
                {
                    var doofuses = doofusesEntities.AsWriter();
                    for (var index = 0; index < eatingDoofuses; index++)
                    {
                        //pickup the meal for this doofus
                        var targetMeal = new EGID(foodIDs[(uint)index], currentFoodGroup);
                        //Set the target meal for this doofus
                        doofuses[index].targetMeal = new EGID(targetMeal.entityID, eatenFoodGroup);

                        //swap this doofus to the eating group so it won't be picked up again
                        _functions.SwapEntityGroup<DoofusEntityDescriptor>(
                            new EGID(doofusesIDs[index], currentDoofusesGroup), eatingDoofusesGroup);
                        //swap the meal to the being eating group, so it won't be picked up again
                        _functions.SwapEntityGroup<FoodEntityDescriptor>(targetMeal, eatenFoodGroup);
                    }
                }
            }
        }

        readonly IEntityFunctions _functions;

        public EntitiesDB entitiesDB { private get; set; }
    }
}
