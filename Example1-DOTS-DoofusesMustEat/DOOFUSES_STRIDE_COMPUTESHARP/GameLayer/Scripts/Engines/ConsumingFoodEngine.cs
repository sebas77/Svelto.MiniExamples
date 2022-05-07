using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Internal;
using Svelto.ECS.MiniExamples.Turrets;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    [Sequenced(nameof(DoofusesEngineNames.ConsumingFoodEngine))]
    public class ConsumingFoodEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public ConsumingFoodEngine(IEntityFunctions nativeFunctions)
        {
            _nativeFunctions = nativeFunctions;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { private get; set; }

        public void Step(in float _param)
        {
            CreateJobForDoofusesAndFood(GameGroups.RED_DOOFUSES_EATING.Groups
                                      , GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup
                                      , GameGroups.RED_FOOD_EATEN.BuildGroup);

            CreateJobForDoofusesAndFood(GameGroups.BLUE_DOOFUSES_EATING.Groups
                                      , GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup
                                      , GameGroups.BLUE_FOOD_EATEN.BuildGroup);
        }

        public string name => nameof(ConsumingFoodEngine);

        void CreateJobForDoofusesAndFood
        (in LocalFasterReadOnlyList<ExclusiveGroupStruct> doofusesEatingGroups
       , ExclusiveGroupStruct doofusesStateGroup, ExclusiveGroupStruct foodStateGroup)
        {
            if (entitiesDB.TryQueryMappedEntities<PositionComponent>(foodStateGroup, out var mappedEntities))
                //against all the doofuses
                foreach (var ((velocities, mealInfos, entityIDs, count), fromGroup) in entitiesDB
                            .QueryEntities<VelocityEntityComponent, MealInfoComponent>(doofusesEatingGroups))
                {
                    var (positions, rotations, _) =
                        entitiesDB.QueryEntities<PositionComponent, RotationComponent>(fromGroup);

                    new ConsumingFoodJob((positions, velocities, mealInfos, rotations, count), entityIDs
                                       , _nativeFunctions, doofusesStateGroup, fromGroup, mappedEntities).Execute();
                }
        }

        readonly IEntityFunctions _nativeFunctions;
    }

    public readonly struct ConsumingFoodJob
    {
        public ConsumingFoodJob
        (in (NB<PositionComponent> positions, NB<VelocityEntityComponent> velocities, NB<MealInfoComponent> mealInfos, NB<RotationComponent> rotations, int count) doofuses
       , NativeEntityIDs nativeEntityIDs, IEntityFunctions entityFunctions
       , ExclusiveBuildGroup doofusesLookingForFoodGroup, ExclusiveGroupStruct doofusesEatingGroup
       , EGIDMapper<PositionComponent> mappedEntities) : this()
        {
            _doofuses                    = doofuses;
            _nativeEntityIDs             = nativeEntityIDs;
            _entityFunctions             = entityFunctions;
            _doofusesLookingForFoodGroup = doofusesLookingForFoodGroup;
            _doofusesEatingGroup         = doofusesEatingGroup;
            _mappedEntities              = mappedEntities;
        }

        public void Execute()
        {
            for (var index = 0; index < _doofuses.count; index++)
            {
                var              mealInfoEGID   = _doofuses.mealInfos[index].targetMeal;
                ref readonly var doofusPosition = ref _doofuses.positions[index].position;
                ref var          velocity       = ref _doofuses.velocities[index].velocity;
                ref var          rotation       = ref _doofuses.rotations[index].rotation;
                ref readonly var foodPosition   = ref _mappedEntities.Entity(mealInfoEGID.entityID).position;

                var computeDirection = foodPosition - doofusPosition;
                var sqrModule = computeDirection.X * computeDirection.X + computeDirection.Z * computeDirection.Z;

                //when it's close enough to the food, it's like the doofus ate it
                if (sqrModule < 0.002f)
                {
                    velocity.X = 0;
                    velocity.Z = 0;

                    //Change Doofus State, won't be looking for food anymore
                    _entityFunctions.SwapEntityGroup<DoofusEntityDescriptor>(
                        new EGID(_nativeEntityIDs[index], _doofusesEatingGroup), _doofusesLookingForFoodGroup);
                    //Remove Eaten Food
                    _entityFunctions.RemoveEntity<FoodEntityDescriptor>(mealInfoEGID);

                    rotation = Quaternion.Identity;
                }
                else
                {
                    //going toward food
                    velocity.X = computeDirection.X;
                    velocity.Z = computeDirection.Z;

                    rotation.LookAt(foodPosition, doofusPosition);
                }
            }
        }

        readonly (NB<PositionComponent> positions, NB<VelocityEntityComponent> velocities, NB<MealInfoComponent> mealInfos, NB<RotationComponent> rotations, int count) _doofuses;

        readonly NativeEntityIDs  _nativeEntityIDs;
        readonly IEntityFunctions _entityFunctions;

        readonly ExclusiveGroupStruct          _doofusesLookingForFoodGroup;
        readonly ExclusiveGroupStruct          _doofusesEatingGroup;
        readonly EGIDMapper<PositionComponent> _mappedEntities;
    }
}