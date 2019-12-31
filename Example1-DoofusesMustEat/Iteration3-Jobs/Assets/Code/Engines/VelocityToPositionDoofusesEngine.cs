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
            var doofuses = entitiesDB
                          .QueryEntities<PositionEntityStruct, VelocityEntityStruct, SpeedEntityStruct
                           >(GameGroups.DOOFUSES)
                          .ToNative<PositionEntityStruct, VelocityEntityStruct, SpeedEntityStruct>().ToBuffers();
            var dep = new Job(doofuses, Time.DeltaTime).Schedule((int) doofuses.length, (int) (doofuses.length / 8), inputDeps);
            
            return new DisposeJob<BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>, NativeBuffer<SpeedEntityStruct>>>(doofuses).Schedule(dep);
        }

        [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
        struct Job : IJobParallelFor
        {
            public Job(
                BufferTuple<NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>,
                    NativeBuffer<SpeedEntityStruct>> doofuses, float deltaTime)
            {
                _doofuses = doofuses;
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