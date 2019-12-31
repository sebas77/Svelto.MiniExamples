using System;
using System.Runtime.InteropServices;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.EntityStructs;
using Svelto.ECS.Extensions.Unity;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1B
{
    [DisableAutoCreation]
    public class RenderingDataSynchronizationEngine : JobComponentSystem, IQueryingEntitiesEngine
    {
        EntityQuery m_Group;
        public IEntitiesDB entitiesDB { get; set; }

        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(typeof(Translation), 
                                     ComponentType.ReadOnly<UnityECSDoofusesGroup>());
        }

        public void Ready() { }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCollection<PositionEntityStruct, NativeBuffer<PositionEntityStruct>> collection =
                entitiesDB.QueryEntities<PositionEntityStruct>(GameGroups.DOOFUSES).ToNative<PositionEntityStruct>();
            EntityCollection<PositionEntityStruct, NativeBuffer<PositionEntityStruct>>.EntityNativeIterator<
                PositionEntityStruct> entityCollection = collection.GetNativeEnumerator<PositionEntityStruct>();

            IntPtr counter = Marshal.AllocHGlobal(sizeof(int));

            var deps = new RenderingSyncJob(entityCollection).Schedule(m_Group, inputDeps);

            return new DisposeJob<EntityCollection<PositionEntityStruct, NativeBuffer<PositionEntityStruct>>.
                EntityNativeIterator<PositionEntityStruct>>(entityCollection).Schedule(deps);
        }
        
        [BurstCompile]
        struct RenderingSyncJob : IJobForEach<Translation, UnityECSDoofusesGroup>
        {
            EntityCollection<PositionEntityStruct, NativeBuffer<PositionEntityStruct>>.EntityNativeIterator<
                PositionEntityStruct> entityCollection;

            public RenderingSyncJob(
                EntityCollection<PositionEntityStruct, NativeBuffer<PositionEntityStruct>>.EntityNativeIterator<
                    PositionEntityStruct> entityCollection)
            {
                this.entityCollection = entityCollection;
            }

            public void Execute(ref Translation translation, [Unity.Collections.ReadOnly] ref UnityECSDoofusesGroup c1)
            {
                ref readonly var positionEntityStruct = ref entityCollection.threadSafeNext.position;

                translation.Value = new float3(positionEntityStruct.x, positionEntityStruct.y, positionEntityStruct.z);
            }
        }
    }
}