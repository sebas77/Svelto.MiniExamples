using Svelto.DataStructures;
using Svelto.ECS.EntityStructs;
using Svelto.ECS.Extensions.Unity;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.MiniExamples.Example1B
{
    [DisableAutoCreation]
    public class VelocityToPositionDoofusesEngine : JobComponentSystem, IQueryingEntitiesEngine
    {
        public void Ready() { }

        public IEntitiesDB entitiesDB { get; set; }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle combinedDependencies = default;
            foreach (var group in GameGroups.DOOFUSES.Groups)
            {
                var doofuses = entitiesDB
                              .QueryEntities<PositionEntityStruct, VelocityEntityStruct, SpeedEntityStruct
                               >(group)
                              .ToNativeBuffers<PositionEntityStruct, VelocityEntityStruct, SpeedEntityStruct>();
                var dep = new ThisSystemJob(doofuses, Time.DeltaTime).Schedule((int) doofuses.count,
                                                                               (int) (doofuses.count / 8), inputDeps);
                
                combinedDependencies =
                    JobHandle.CombineDependencies(combinedDependencies, 
                                                  new DisposeJob<BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>,
                    NativeBuffer<SpeedEntityStruct>>>(doofuses).Schedule(dep));
            }

            return combinedDependencies;
        }

        [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
        struct ThisSystemJob : IJobParallelFor
        {
            public ThisSystemJob(
                BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>,
                    NativeBuffer<SpeedEntityStruct>> doofuses, float deltaTime)
            {
                _doofuses  = doofuses;
                _deltaTime = deltaTime;
            }

            public void Execute(int index)
            {
                var ecsVector3 = _doofuses.buffer2[index].velocity;

                _doofuses.buffer1[index].position += (ecsVector3 * (_deltaTime * _doofuses.buffer3[index].speed));
            }

            readonly float _deltaTime;

            readonly BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>,
                NativeBuffer<SpeedEntityStruct>> _doofuses;
        }
    }
}