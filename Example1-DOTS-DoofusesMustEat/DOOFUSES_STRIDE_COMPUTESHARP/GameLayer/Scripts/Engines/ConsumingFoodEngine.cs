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

        public void Ready()
        {
        }

        public ConsumingFoodEngine(IEntityFunctions nativeFunctions)
        {
            _nativeFunctions = nativeFunctions;
        }

        public void Step(in float _param)
        {
            CreateJobForDoofusesAndFood(GameGroups.RED_DOOFUSES_EATING.Groups,
                GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup);

            CreateJobForDoofusesAndFood(GameGroups.BLUE_DOOFUSES_EATING.Groups,
                GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup);
        }

        public string name => nameof(ConsumingFoodEngine);

        void CreateJobForDoofusesAndFood(in LocalFasterReadOnlyList<ExclusiveGroupStruct> doofusesEatingGroups,
            ExclusiveBuildGroup foodEatenGroup)
        {
            //against all the doofuses
            foreach (var (doofusesBuffer, fromGroup) in entitiesDB
                        .QueryEntities<PositionComponent, VelocityEntityComponent, MealInfoComponent>(
                             doofusesEatingGroups))
            {
                var (buffer1, buffer2, buffer3, entityIDs, count) = doofusesBuffer;

                new ConsumingFoodJob((buffer1, buffer2, buffer3, count), entityIDs, _nativeFunctions, entitiesDB,
                    foodEatenGroup, fromGroup).Execute();
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

        readonly ExclusiveBuildGroup  _doofuseMealLockedGroup;
        readonly ExclusiveGroupStruct _doofuseFromGroup;

        public ConsumingFoodJob(
            in BT<NB<PositionComponent>, NB<VelocityEntityComponent>, NB<MealInfoComponent>> doofuses,
            NativeEntityIDs nativeEntityIDs, IEntityFunctions entityFunctions, EntitiesDB entitiesDb,
            ExclusiveBuildGroup doofuseMealLockedGroup, ExclusiveGroupStruct doofuseFromGroup) : this()
        {
            _doofuses               = doofuses;
            _nativeEntityIDs        = nativeEntityIDs;
            _entityFunctions        = entityFunctions;
            _entitiesDb             = entitiesDb;
            _doofuseMealLockedGroup = doofuseMealLockedGroup;
            _doofuseFromGroup       = doofuseFromGroup;
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

                //close enough to the food
                if (sqrModule < 0.2f)
                {
                    velocity.X = 0;
                    velocity.Z = 0;

                    //food found
                    //Change Doofuses State
                    _entityFunctions.SwapEntityGroup<DoofusEntityDescriptor>(
                        new EGID(_nativeEntityIDs[index], _doofuseFromGroup), _doofuseMealLockedGroup);
                    //Remove Eaten Food
                    _entityFunctions.RemoveEntity<FoodEntityDescriptor>(mealInfoEGID);
                }

                //going toward food, not breaking as closer food can spawn
                velocity.X = computeDirection.X;
                velocity.Z = computeDirection.Z;
            }
        }
    }
}