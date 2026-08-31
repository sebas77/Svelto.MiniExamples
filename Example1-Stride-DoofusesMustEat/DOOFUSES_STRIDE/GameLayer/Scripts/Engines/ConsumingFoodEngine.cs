using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Internal;
using Svelto.ECS.MiniExamples.Turrets;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    [Sequenced(nameof(DoofusesEngineNames.ConsumingFoodEngine))]
    public class ConsumingFoodEngine: IQueryingEntitiesEngine, IUpdateEngine
    {
        readonly IEntityFunctions _nativeFunctions;

        public void Ready() { }

        public ConsumingFoodEngine(IEntityFunctions nativeFunctions)
        {
            _nativeFunctions = nativeFunctions;
        }

        public void Step(in float _param)
        {
            CreateJobForDoofusesAndFood(
                GameGroups.RED_DOOFUSES_EATING.Groups
              , GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup
              , GameGroups.RED_FOOD_EATEN.BuildGroup);

            CreateJobForDoofusesAndFood(
                GameGroups.BLUE_DOOFUSES_EATING.Groups
              , GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup
              , GameGroups.BLUE_FOOD_EATEN.BuildGroup);
        }

        public string name => nameof(ConsumingFoodEngine);

        void CreateJobForDoofusesAndFood(in LocalFasterReadOnlyList<ExclusiveGroupStruct> doofusesEatingGroups
          , ExclusiveGroupStruct doofusesStateGroup, ExclusiveGroupStruct foodStateGroup)
        {
            if (entitiesDB.TryQueryMappedEntities<PositionComponent>(foodStateGroup, out var mappedEntities))
            {
                //against all the doofuses
                foreach (var ((positions, velocities, rotations, entityIDs, count), fromGroup) in entitiesDB
                                .QueryEntities<PositionComponent, VelocityComponent,
                                     RotationComponent>(doofusesEatingGroups))
                {
                    var (mealInfos, _) = entitiesDB.QueryEntities<MealInfoComponent>(fromGroup);

                    var mealInfoReader = mealInfos.AsReader();
                    var positionReader  = positions.AsReader();
                    var velocityWriter  = velocities.AsWriter();
                    var rotationWriter  = rotations.AsWriter();
                    for (var index = 0; index < count; index++)
                    {
                        EGID mealInfoEGID = mealInfoReader[index].targetMeal;
                        ref readonly var doofusPosition = ref positionReader[index].position;
                        ref var velocity = ref velocityWriter[index].velocity;
                        ref var rotation = ref rotationWriter[index].rotation;
                        ref readonly var foodPosition = ref mappedEntities.Entity(mealInfoEGID.entityID).position;

                        var sourcePoint = new Vector3(foodPosition.X, foodPosition.Y, foodPosition.Z);
                        var destPoint = new Vector3(doofusPosition.X, doofusPosition.Y, doofusPosition.Z);
                        var computeDirection = sourcePoint - destPoint;
                        var sqrModule = computeDirection.X * computeDirection.X + computeDirection.Z * computeDirection.Z;

                        //when it's close enough to the food, it's like the doofus ate it
                        if (sqrModule < 0.002f)
                        {
                            velocity.X = 0;
                            velocity.Z = 0;

                            //Change Doofus State, won't be looking for food anymore
                            _nativeFunctions.SwapEntityGroup<DoofusEntityDescriptor>(
                                new EGID(entityIDs[index], fromGroup), doofusesStateGroup);
                            //Remove Eaten Food
                            _nativeFunctions.RemoveEntity<FoodEntityDescriptor>(mealInfoEGID);

                            rotation = Quaternion.Identity;
                        }
                        else
                        {
                            //going toward food
                            velocity.X = computeDirection.X;
                            velocity.Z = computeDirection.Z;

                            rotation.LookAt(sourcePoint, destPoint);
                        }
                    }
                }
            }
        }

        public EntitiesDB entitiesDB { private get; set; }
    }
}
