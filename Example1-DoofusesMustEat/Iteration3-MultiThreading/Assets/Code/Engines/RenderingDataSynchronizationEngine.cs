using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [DisableAutoCreation]
    public class RenderingDataSynchronizationEngine: ComponentSystem, IQueryingEntitiesEngine
    { 
        public IEntitiesDB entitiesDB { get; set; }
        public void Ready() {  }

        protected override void OnUpdate()
        {
            if (entitiesDB == null) return;
            
            var positionEntityStructs = entitiesDB.QueryEntities<PositionEntityStruct>(GameGroups.DOOFUSES, out var count);
            
            var positionGC    = GCHandle.Alloc(positionEntityStructs, GCHandleType.Pinned);

            NewMethod(positionGC, count);
            
            positionGC.Free();
            
        }

        unsafe void NewMethod(GCHandle positionGC, uint count)
        {
            var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<PositionEntityStruct>(
                (void*) positionGC.AddrOfPinnedObject(), (int) count, Allocator.TempJob);

            new MovementJob(arr).Schedule(this).Complete();
            
            arr.Dispose();
        }

        [BurstCompile]
        struct MovementJob : IJobProcessComponentData<Translation, UnityECSDoofusesGroup>
        {
            public MovementJob(NativeArray<PositionEntityStruct> positions) { _positions = positions; }
            
            public void Execute(ref Translation Translation, [ReadOnly] ref UnityECSDoofusesGroup rotation)
            {
                var positionEntityStruct = _positions[index];
                Translation.Value = new float3(positionEntityStruct.position.x, positionEntityStruct.position.y,
                                               positionEntityStruct.position.z);
                                    
                
                Interlocked.Add(ref index, 1);
            }

            static int index = 0;

            readonly NativeArray<PositionEntityStruct> _positions;
        }
    }
}