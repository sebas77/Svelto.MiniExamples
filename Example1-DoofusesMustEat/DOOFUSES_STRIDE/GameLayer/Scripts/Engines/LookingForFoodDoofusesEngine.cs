using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    [Sequenced(nameof(DoofusesEngineNames.LookingForFoodDoofusesEngine))]
    public class LookingForFoodDoofusesEngine : IQueryingEntitiesEngine, IUpdateEngine
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
            CreateJobForDoofusesAndFood(GameGroups.RED_FOOD_NOT_EATEN.Groups
                                      , GameGroups.RED_DOOFUSES_NOT_EATING.Groups
                                      , GameGroups.RED_DOOFUSES_EATING.BuildGroup
                                      , GameGroups.RED_FOOD_EATEN.BuildGroup);

            //Iterate NOEATING BLUE doofuses to look for BLUE food and MOVE them to EATING state if food is found
            CreateJobForDoofusesAndFood(GameGroups.BLUE_FOOD_NOT_EATEN.Groups
                                      , GameGroups.BLUE_DOOFUSES_NOT_EATING.Groups
                                      , GameGroups.BLUE_DOOFUSES_EATING.BuildGroup
                                      , GameGroups.BLUE_FOOD_EATEN.BuildGroup);
        }

        /// <summary>
        /// All the available doofuses will start to hunt for available food
        /// </summary>
        void CreateJobForDoofusesAndFood
        (FasterReadOnlyList<ExclusiveGroupStruct> availableFood
       , FasterReadOnlyList<ExclusiveGroupStruct> availableDoofuses, ExclusiveBuildGroup eatingDoofusesGroup
       , ExclusiveBuildGroup eatenFoodGroup)
        {
            foreach (var ((_, foodEntities, availableFoodCount), fromGroup) in entitiesDB
                        .QueryEntities<PositionComponent>(availableFood))
            {
                foreach (var ((doofuses, egids, doofusesCount), fromDoofusesGroup) in entitiesDB
                            .QueryEntities<MealInfoComponent>(availableDoofuses))
                {
                    var willEatDoofusesCount = Math.Min(availableFoodCount, doofusesCount);

                    //schedule the job
                    new LookingForFoodDoofusesJob()
                    {
                        _doofuses                    = doofuses
                      , _doofusesegids               = egids
                      , _food                        = foodEntities
                      , _functions                   = _functions
                      , _doofusesEatingGroup         = eatingDoofusesGroup
                      , _lockedFood                  = eatenFoodGroup
                      , _fromFoodGroup               = fromGroup
                      , _doofusesLookingForFoodGroup = fromDoofusesGroup
                      , _count                       = willEatDoofusesCount
                    }.Execute();
                }
            }
        }

        readonly IEntityFunctions _functions;

        public EntitiesDB entitiesDB { private get; set; }

        struct LookingForFoodDoofusesJob
        {
            public NB<MealInfoComponent> _doofuses;
            public NativeEntityIDs       _food;
            public ExclusiveBuildGroup   _doofusesEatingGroup;
            public ExclusiveBuildGroup   _lockedFood;
            public NativeEntityIDs       _doofusesegids;
            public ExclusiveGroupStruct  _fromFoodGroup;
            public ExclusiveGroupStruct  _doofusesLookingForFoodGroup;
            public IEntityFunctions      _functions;
            public int                   _count;

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