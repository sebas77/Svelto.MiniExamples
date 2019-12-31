using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class RenderingDataSynchronizationEngine: IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public RenderingDataSynchronizationEngine(World world)
        {
            _group       = world.EntityManager.CreateEntityQuery(typeof(Translation), typeof(UnityECSDoofusesGroup));
        }

        public void Ready()
        {
            SynchronizeUnityECSEntitiesWithSveltoECSEntities().RunOn(DoofusesStandardSchedulers.rendererScheduler);
        }

        IEnumerator SynchronizeUnityECSEntitiesWithSveltoECSEntities()
        {
            while (true)
            {
                var calculateLength = _group.CalculateEntityCount();
                
                var positionEntityStructs =
                    entitiesDB.QueryEntities<PositionEntityStruct>(GameGroups.DOOFUSES, out var count);

#if DEBUG && !PROFILER                
                if (calculateLength != count) {yield return null; continue;}
#endif

                var positions =
                    new NativeArray<Translation>(calculateLength, Allocator.TempJob,
                                                 NativeArrayOptions.UninitializedMemory);
                
                for (int index = 0; index < count; index++)
                {
                    positions[index] = new Translation
                    {
                        Value = new float3(positionEntityStructs[index].position.x,
                                           positionEntityStructs[index].position.y,
                                           positionEntityStructs[index].position.z)
                    };
                }
                //UnityECS: this is not the proper way to sync with UnityECS. I will update when I have the time
                //but for the purpose of this example, it doesn't matter
                _group.CopyFromComponentDataArray(positions, out var handle3);

                handle3.Complete();
                positions.Dispose();

                yield return null;
            }
        }

        readonly EntityQuery      _group;
    }
}