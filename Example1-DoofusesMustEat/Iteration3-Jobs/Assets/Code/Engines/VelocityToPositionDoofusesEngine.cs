using Svelto.DataStructures;
using Svelto.ECS.EntityStructs;
using Svelto.ECS.Extensions.Unity;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [DisableAutoCreation]
    public class VelocityToPositionDoofusesEngine : SystemBase, IQueryingEntitiesEngine
    {
        public void Ready()
        { }

        public EntitiesDB entitiesDB { get; set; }

        protected override void OnUpdate()
        {
            foreach (var group in GameGroups.DOOFUSES.Groups)
            {
                var doofuses = entitiesDB
                    .QueryEntities<PositionEntityStruct, VelocityEntityStruct, SpeedEntityStruct>(group)
                    .ToNativeBuffers<PositionEntityStruct, VelocityEntityStruct, SpeedEntityStruct>();
                var dep = new ThisSystemJob(doofuses, Time.DeltaTime).Schedule((int) doofuses.count,
                    (int) (doofuses.count / 8), Dependency);

                Dependency = new DisposeJob<BufferTuple<
                        NativeBuffer<PositionEntityStruct>, NativeBuffer<VelocityEntityStruct>,
                        NativeBuffer<SpeedEntityStruct>>>(doofuses).Schedule(dep);
            }
        }

        struct ThisSystemJob : IJobParallelFor
        {
            public ThisSystemJob(
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