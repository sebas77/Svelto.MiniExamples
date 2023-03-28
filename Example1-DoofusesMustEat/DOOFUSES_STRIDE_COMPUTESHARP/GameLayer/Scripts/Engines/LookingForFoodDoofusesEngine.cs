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
                    new LookingForFoodDoofusesJob()
                    {
                        _doofuses = doofusesEntities,
                        _doofusesegids = doofusesIDs,
                        _food = foodIDs,
                        _functions = _functions,
                        _doofusesEatingGroup = eatingDoofusesGroup,
                        _lockedFood = eatenFoodGroup,
                        _fromFoodGroup = currentFoodGroup,
                        _doofusesLookingForFoodGroup = currentDoofusesGroup,
                        _count = (int)eatingDoofuses
                    }.Execute();
                }
            }
        }

        readonly IEntityFunctions _functions;

        public EntitiesDB entitiesDB { private get; set; }

        struct LookingForFoodDoofusesJob
        {
            public NB<MealInfoComponent> _doofuses;
            public NativeEntityIDs _food;
            public ExclusiveBuildGroup _doofusesEatingGroup;
            public ExclusiveBuildGroup _lockedFood;
            public NativeEntityIDs _doofusesegids;
            public ExclusiveGroupStruct _fromFoodGroup;
            public ExclusiveGroupStruct _doofusesLookingForFoodGroup;
            public IEntityFunctions _functions;
            public int _count;

            public void Execute()
            {
                for (int index = 0; index < _count; index++)
                {
                    //pickup the meal for this doofus
                    var targetMeal = new EGID(_food[(uint)index], _fromFoodGroup);
                    //Set the target meal for this doofus
                    _doofuses[index].targetMeal = new EGID(targetMeal.entityID, _lockedFood);

                    //swap this doofus to the eating group so it won't be picked up again
                    _functions.SwapEntityGroup<DoofusEntityDescriptor>(
                        new EGID(@_doofusesegids[index], _doofusesLookingForFoodGroup), _doofusesEatingGroup);
                    //swap the meal to the being eating group, so it won't be picked up again
                    _functions.SwapEntityGroup<FoodEntityDescriptor>(targetMeal, _lockedFood);
                }
            }
        }
    }
}