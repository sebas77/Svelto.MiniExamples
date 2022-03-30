// using Svelto.Common;
// using Svelto.DataStructures;
// using Svelto.ECS.Internal;
//
// namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
// {
//     [Sequenced(nameof(DoofusesEngineNames.LookingForFoodDoofusesEngine))]
//     public class LookingForFoodDoofusesEngine : IQueryingEntitiesEngine, IUpdateEngine
//     {
//         public void Ready() { }
//
//         public LookingForFoodDoofusesEngine(IEntityFunctions nativeSwap)
//         {
//             _nativeDoofusesSwap = nativeSwap.ToNativeSwap<DoofusEntityDescriptor>(nameof(LookingForFoodDoofusesEngine));
//             _nativeFoodSwap     = nativeSwap.ToNativeSwap<FoodEntityDescriptor>(nameof(LookingForFoodDoofusesEngine));
//         }
//
//         public string name => nameof(LookingForFoodDoofusesEngine);
//
//         public void Step(in float _param)
//         {
//             //Iterate NOEATING RED doofuses to look for RED food and MOVE them to EATING state if food is found
//             var handle1 = CreateJobForDoofusesAndFood(inputDeps, GameGroups.RED_FOOD_NOT_EATEN.Groups
//                                                     , GameGroups.RED_DOOFUSES_NOT_EATING.Groups
//                                                     , GameGroups.RED_DOOFUSES_EATING.BuildGroup
//                                                     , GameGroups.RED_FOOD_EATEN.BuildGroup);
//
//             //Iterate NOEATING BLUE doofuses to look for BLUE food and MOVE them to EATING state if food is found
//             var handle2 = CreateJobForDoofusesAndFood(inputDeps, GameGroups.BLUE_FOOD_NOT_EATEN.Groups
//                                                     , GameGroups.BLUE_DOOFUSES_NOT_EATING.Groups
//                                                     , GameGroups.BLUE_DOOFUSES_EATING.BuildGroup
//                                                     , GameGroups.BLUE_FOOD_EATEN.BuildGroup);
//
//             //can run in parallel
//             return JobHandle.CombineDependencies(handle1, handle2);
//         }
//
//         /// <summary>
//         /// All the available doofuses will start to hunt for available food
//         /// </summary>
//         JobHandle CreateJobForDoofusesAndFood
//         (JobHandle inputDeps, FasterReadOnlyList<ExclusiveGroupStruct> availableFood
//        , FasterReadOnlyList<ExclusiveGroupStruct> availableDoofuses, ExclusiveBuildGroup eatingDoofusesGroup
//        , ExclusiveBuildGroup eatenFoodGroup)
//         {
//             JobHandle combinedDeps = default;
//
//             foreach (var ((_, foodEntities, availableFoodCount), fromGroup) in entitiesDB.QueryEntities<PositionEntityComponent>(
//                 availableFood))
//             {
//                 foreach (var ((doofuses, egids, doofusesCount), fromDoofusesGroup) in entitiesDB
//                    .QueryEntities<MealInfoComponent>(availableDoofuses))
//                 {
//                     var willEatDoofuses = math.min(availableFoodCount, doofusesCount);
//
//                     //schedule the job
//                     combinedDeps = JobHandle.CombineDependencies(inputDeps, new LookingForFoodDoofusesJob()
//                     {
//                         _doofuses               = doofuses
//                       , _doofusesegids          = egids
//                       , _food                   = foodEntities
//                       , _nativeDoofusesSwap     = _nativeDoofusesSwap
//                       , _nativeFoodSwap         = _nativeFoodSwap
//                       , _doofuseMealLockedGroup = eatingDoofusesGroup
//                       , _lockedFood             = eatenFoodGroup
//                         , _fromFoodGroup        = fromGroup
//                           , _fromDoofusesGroup  = fromDoofusesGroup
//                     }.ScheduleParallel(willEatDoofuses, inputDeps));
//                 }
//             }
//
//             return combinedDeps;
//         }
//
//         readonly NativeEntitySwap _nativeDoofusesSwap;
//         readonly NativeEntitySwap _nativeFoodSwap;
//
//         public EntitiesDB entitiesDB { private get; set; }
//
//         [BurstCompile]
//         struct LookingForFoodDoofusesJob : IJobParallelFor
//         {
//             public NB<MealInfoComponent> _doofuses;
//             public NativeEntityIDs       _food;
//             public NativeEntitySwap      _nativeDoofusesSwap;
//             public NativeEntitySwap      _nativeFoodSwap;
//             public ExclusiveBuildGroup   _doofuseMealLockedGroup;
//             public ExclusiveBuildGroup   _lockedFood;
//             public NativeEntityIDs       _doofusesegids;
//             public ExclusiveGroupStruct  _fromFoodGroup;
//             public ExclusiveGroupStruct  _fromDoofusesGroup;
//
//             public void Execute(int index)
//             {
//                 //pickup the meal for this doofus
//                 var targetMeal = new EGID(_food[(uint) index], _fromFoodGroup);
//                 //Set the target meal for this doofus
//                 _doofuses[index].targetMeal = new EGID(targetMeal.entityID, _lockedFood);
//
//                 //swap this doofus to the eating group so it won't be picked up again
//                 _nativeDoofusesSwap.SwapEntity(new EGID(@_doofusesegids[index], _fromDoofusesGroup), _doofuseMealLockedGroup, _threadIndex);
//                 //swap the meal to the being eating group, so it won't be picked up again
//                 _nativeFoodSwap.SwapEntity(targetMeal, _lockedFood);
//             }
//         }
//     }
// }