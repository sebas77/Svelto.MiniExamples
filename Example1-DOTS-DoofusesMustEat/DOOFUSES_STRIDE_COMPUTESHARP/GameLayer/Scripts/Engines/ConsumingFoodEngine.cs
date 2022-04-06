using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Internal;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    [Sequenced(nameof(DoofusesEngineNames.ConsumingFoodEngine))]
    public class ConsumingFoodEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        readonly IEntityFunctions _nativeFunctions;

        public void Ready() { }

        public ConsumingFoodEngine(IEntityFunctions nativeFunctions)
        {
            _nativeFunctions = nativeFunctions;
        }

        public void Step(in float _param)
        {
            CreateJobForDoofusesAndFood(GameGroups.RED_DOOFUSES_EATING.Groups
                                      , GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup);

            CreateJobForDoofusesAndFood(GameGroups.BLUE_DOOFUSES_EATING.Groups
                                      , GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup);
        }

        public string name => nameof(ConsumingFoodEngine);

        void CreateJobForDoofusesAndFood
            (in LocalFasterReadOnlyList<ExclusiveGroupStruct> doofusesEatingGroups, ExclusiveBuildGroup foodEatenGroup)
        {
            //against all the doofuses
            foreach (var ((buffer1, buffer2, buffer3, entityIDs, count), fromGroup) in entitiesDB
                        .QueryEntities<PositionComponent, VelocityEntityComponent, MealInfoComponent>(
                             doofusesEatingGroups))
            {
                new ConsumingFoodJob((buffer1, buffer2, buffer3, count), entityIDs, _nativeFunctions, entitiesDB
                                   , foodEatenGroup, fromGroup).Execute();
            }
        }

        public EntitiesDB entitiesDB { private get; set; }
    }

    public readonly struct ConsumingFoodJob
    {
        readonly BT<NB<PositionComponent>, NB<VelocityEntityComponent>, NB<MealInfoComponent>> _doofuses;

        readonly NativeEntityIDs  _nativeEntityIDs;
        readonly IEntityFunctions _entityFunctions;
        readonly EntitiesDB       _entitiesDb;

        readonly ExclusiveGroupStruct _doofusesLookingForFoodGroup;
        readonly ExclusiveGroupStruct _doofusesEatingGroup;

        public ConsumingFoodJob
        (in BT<NB<PositionComponent>, NB<VelocityEntityComponent>, NB<MealInfoComponent>> doofuses
       , NativeEntityIDs nativeEntityIDs, IEntityFunctions entityFunctions, EntitiesDB entitiesDb
       , ExclusiveBuildGroup doofusesLookingForFoodGroup, ExclusiveGroupStruct doofusesEatingGroup) : this()
        {
            _doofuses                    = doofuses;
            _nativeEntityIDs             = nativeEntityIDs;
            _entityFunctions             = entityFunctions;
            _entitiesDb                  = entitiesDb;
            _doofusesLookingForFoodGroup = doofusesLookingForFoodGroup;
            _doofusesEatingGroup         = doofusesEatingGroup;
        }

        public void Execute()
        {
            for (var index = 0; index < _doofuses.count; index++)
            {
                ref EGID    mealInfoEGID   = ref _doofuses.buffer3[index].targetMeal;
                ref Vector3 doofusPosition = ref _doofuses.buffer1[index].position;
                ref Vector3 velocity       = ref _doofuses.buffer2[index].velocity;
                ref Vector3 foodPosition   = ref _entitiesDb.QueryEntity<PositionComponent>(mealInfoEGID).position;

                var computeDirection = foodPosition - doofusPosition;
                var sqrModule = computeDirection.X * computeDirection.X + computeDirection.Z * computeDirection.Z;

                //when it's close enough to the food, it's like the doofus ate it
                if (sqrModule < 0.00002f)
                {
                    velocity.X = 0;
                    velocity.Z = 0;

                    //Change Doofus State, won't be looking for food anymore
                    _entityFunctions.SwapEntityGroup<DoofusEntityDescriptor>(
                        new EGID(_nativeEntityIDs[index], _doofusesEatingGroup), _doofusesLookingForFoodGroup);
                    //Remove Eaten Food
                    _entityFunctions.RemoveEntity<FoodEntityDescriptor>(mealInfoEGID);
                }
                else
                {
                    //going toward food
                    velocity.X = computeDirection.X;
                    velocity.Z = computeDirection.Z;
                }
            }
        }
    }
}