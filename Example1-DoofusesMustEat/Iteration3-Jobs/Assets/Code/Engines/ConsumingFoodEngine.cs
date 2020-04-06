using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Extensions.Unity;
using Svelto.Tasks.Enumerators;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.ConsumingFoodEngine))]
    public class ConsumingFoodEngine : IQueryingEntitiesEngine, IJobifiableEngine
    {
        public ConsumingFoodEngine(IEntityFunctions entityFunctions)
        {
            _nativeEntityRemove =
                entityFunctions.ToNativeRemove<FoodEntityDescriptor>();
        }

        public EntitiesDB entitiesDB { private get; set; }

        public void Ready() { }

//        ReusableWaitForSecondsEnumerator _wait = new ReusableWaitForSecondsEnumerator(0.1f);
        readonly NativeEntityRemove _nativeEntityRemove;

        public JobHandle Execute(JobHandle _jobHandle)
        {
            // while (_wait.IsDone() == false)
            //     return _jobHandle;
            
            foreach (var group in GameGroups.FOOD.Groups)
            {
                var buffer =
                    entitiesDB.NativeEntitiesBuffer<MealEntityComponent>(group, out uint count);

                var parallelJob = new ParallelJob(buffer, _nativeEntityRemove);
                var scheduled = parallelJob.Schedule((int) count, ProcessorCount.Batch(count), _jobHandle);

                _jobHandle = buffer.CombineDispose(scheduled, _jobHandle);
            }

            return _jobHandle;
        }
    }

    [BurstCompile]
    struct ParallelJob : IJobParallelFor
    {
#pragma warning disable 649
        [NativeSetThreadIndex] int threadIndex;
#pragma warning restore 649
        public ParallelJob
            (in NativeBuffer<MealEntityComponent> buffers, NativeEntityRemove nativeEntityRemove) : this()
        {
            _buffers                = buffers;
            _nativeEntityRemove = nativeEntityRemove;
        }

        public void Execute(int index)
        {
            ref var mealStructs = ref _buffers[index];

            mealStructs.mealLeft -= mealStructs.eaters;
            mealStructs.eaters   =  0;

            if (mealStructs.eaters > 0)
                mealStructs.mealLeft = 0;

            if (mealStructs.mealLeft <= 0)
                _nativeEntityRemove.RemoveEntity(mealStructs.ID, threadIndex + 1);
        }

        NativeBuffer<MealEntityComponent> _buffers;
        readonly NativeEntityRemove   _nativeEntityRemove;
    }
}