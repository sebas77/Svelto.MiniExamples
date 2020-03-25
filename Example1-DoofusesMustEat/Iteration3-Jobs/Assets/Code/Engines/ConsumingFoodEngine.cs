using Svelto.DataStructures;
using Svelto.Tasks.Enumerators;
using Unity.Burst;
using Unity.Jobs;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class ConsumingFoodEngine : IQueryingEntitiesEngine, IJobifiableEngine
    {
        public ConsumingFoodEngine(IEntityFunctions entityFunctions)
        {
            _entityFunctions = entityFunctions;
        }

        public EntitiesDB entitiesDB { private get; set; }

        public void Ready() { }

        readonly IEntityFunctions _entityFunctions;
        ReusableWaitForSecondsEnumerator _wait = new ReusableWaitForSecondsEnumerator(0.1f);

        public JobHandle Execute(JobHandle _jobHandle)
        {
            while (_wait.IsDone() == false) return _jobHandle;

            NativeEntityFunction entityRemover = _entityFunctions.ToNative<FoodEntityDescriptor>();

            foreach (var group in GameGroups.FOOD.Groups)
            {
                NativeBuffer<MealEntityStruct> buffer =
                    entitiesDB.NativeEntitiesBuffer<MealEntityStruct>(group, out uint count);

                _jobHandle =
                    JobHandle.CombineDependencies(_jobHandle,new ParallelJob(buffer, entityRemover).Schedule((int) count,
                    (int) (count / 8), _jobHandle));

                _jobHandle = buffer.CombinedDispose(_jobHandle, _jobHandle);
            }

            return _jobHandle;
        }
    }

    [BurstCompile]
    struct ParallelJob : IJobParallelFor
    {
        public ParallelJob(in NativeBuffer<MealEntityStruct> buffers, NativeEntityFunction nativeEntityFunction)
        {
            _buffers = buffers;
            _nativeEntityFunction = nativeEntityFunction;
        }

        public void Execute(int index)
        {
            ref var mealStructs = ref _buffers[index];

            mealStructs.mealLeft -= mealStructs.eaters;
            mealStructs.eaters = 0;

            if (mealStructs.mealLeft <= 0) _nativeEntityFunction.RemoveEntity(mealStructs.ID);
        }

        NativeBuffer<MealEntityStruct> _buffers;
        NativeEntityFunction _nativeEntityFunction;
    }
}