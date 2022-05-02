// using Svelto.Common;
// using Svelto.DataStructures;
// using Svelto.ECS.EntityComponents;
//
// namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
// {
//     [Sequenced(nameof(DoofusesEngineNames.ConsumingFoodEngine))]
//     public class ComputeDistancesEngine : IQueryingEntitiesEngine, IUpdateEngine
//     {
//         public void Ready() { }
//
//         public void Step(in float _param)
//         {
//             CreateJobForDoofusesAndFood(GameGroups.RED_DOOFUSES_EATING.Groups, GameGroups.RED_FOOD_EATEN.BuildGroup);
//
//             CreateJobForDoofusesAndFood(GameGroups.BLUE_DOOFUSES_EATING.Groups, GameGroups.BLUE_FOOD_EATEN.BuildGroup);
//         }
//
//         public string name => nameof(ConsumingFoodEngine);
//
//         void CreateJobForDoofusesAndFood
//         (in LocalFasterReadOnlyList<ExclusiveGroupStruct> doofusesEatingGroups, ExclusiveGroupStruct foodStateGroup)
//         {
//             if (entitiesDB.TryQueryMappedEntities<PositionComponent>(foodStateGroup, out var mappedEntities))
//             {
//                 //against all the doofuses
//                 foreach (var ((positions, velocities, mealInfos, rotations, count), _) in entitiesDB
//                             .QueryEntities<PositionComponent, VelocityEntityComponent, MealInfoComponent,
//                                  RotationComponent>(doofusesEatingGroups))
//                 {
//                     new ComputeDistancesJob((positions, velocities, mealInfos, rotations, count), mappedEntities).Execute();
//                 }
//             }
//         }
//
//         public EntitiesDB entitiesDB { private get; set; }
//         
//         public readonly struct ComputeDistancesJob
//         {
//             readonly BT<NB<PositionComponent>, NB<VelocityEntityComponent>, NB<MealInfoComponent>, NB<RotationComponent>>
//                 _doofuses;
//
//             readonly EGIDMapper<PositionComponent> _mappedEntities;
//
//             public ComputeDistancesJob
//             (in (NB<PositionComponent> positions, NB<VelocityEntityComponent> velocities, NB<MealInfoComponent>
//                  mealInfos, NB<RotationComponent> rotations, int count) doofuses, EGIDMapper<PositionComponent> mappedEntities) : this()
//             {
//                 _doofuses       = doofuses;
//                 _mappedEntities = mappedEntities;
//             }
//
//             public void Execute()
//             {
//                 for (var index = 0; index < _doofuses.count; index++)
//                 {
//                     var mealInfoComponent = _doofuses.buffer3[index];
//                 
//                     EGID             mealInfoEGID   = mealInfoComponent.targetMeal;
//                     ref readonly var doofusPosition = ref _doofuses.buffer1[index].position;
//                     ref readonly var foodPosition   = ref _mappedEntities.Entity(mealInfoEGID.entityID).position;
//
//                     var computeDirection = foodPosition - doofusPosition;
//                     var sqrModule = computeDirection.X * computeDirection.X + computeDirection.Z * computeDirection.Z;
//                 }
//             }
//         }
//     }
// }