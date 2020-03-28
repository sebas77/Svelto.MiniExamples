using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Svelto.Tasks.Enumerators;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.ConsumingFoodEngine))]
    public class ConsumingFoodEngine : IQueryingEntitiesEngine, IJobifiableEngine
    {
        public ConsumingFoodEngine(IEntityFunctions entityFunctions) { _entityFunctions = entityFunctions; }

        public EntitiesDB entitiesDB { private get; set; }

        public void Ready() { }

        ReusableWaitForSecondsEnumerator _wait = new ReusableWaitForSecondsEnumerator(0.1f);
        readonly IEntityFunctions        _entityFunctions;

        public JobHandle Execute(JobHandle _jobHandle)
        {
            while (_wait.IsDone() == false)
                return _jobHandle;

            NativeEntityOperations _nativeEntityOperations =
                _entityFunctions.ToNative<FoodEntityDescriptor>(Allocator.TempJob);

            foreach (var group in GameGroups.FOOD.Groups)
            {
                NativeBuffer<MealEntityStruct> buffer =
                    entitiesDB.NativeEntitiesBuffer<MealEntityStruct>(group, out uint count);

                _jobHandle = JobHandle.CombineDependencies(
                    _jobHandle
                  , new ParallelJob(buffer, _nativeEntityOperations).Schedule(
                        (int) count, ProcessorCount.Batch(count)));

                _jobHandle = buffer.CombineDispose(_jobHandle, _jobHandle);
            }

            return _jobHandle;
        }
    }

    [BurstCompile]
    struct ParallelJob : IJobParallelFor
    {
        public ParallelJob
            (in NativeBuffer<MealEntityStruct> buffers, NativeEntityOperations nativeEntityOperations) : this()
        {
            _buffers                = buffers;
            _nativeEntityOperations = nativeEntityOperations;
        }

        public void Execute(int index)
        {
            ref var mealStructs = ref _buffers[index];

            mealStructs.mealLeft -= mealStructs.eaters;
            mealStructs.eaters   =  0;

            if (mealStructs.mealLeft <= 0)
                _nativeEntityOperations.RemoveEntity(mealStructs.ID);
        }

        NativeBuffer<MealEntityStruct> _buffers;
        NativeEntityOperations         _nativeEntityOperations;
    }
}