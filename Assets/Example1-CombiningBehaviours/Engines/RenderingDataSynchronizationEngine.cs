using System;
using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Svelto.Tasks.ExtraLean.Unity;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1
{
    [DisableAutoCreation]
    public class RenderingDataSynchronizationEngine
        : SingleEntityEngine<UnityECSEntityStruct>, IDisposable, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public RenderingDataSynchronizationEngine(World world)
        {
            _runner      = new CoroutineMonoRunner("test");
            _uecsManager = world.EntityManager;
            _group       = world.EntityManager.CreateComponentGroup(typeof(Translation), typeof(UnityECSDoofusesGroup));
        }

        public void Ready() { SynchronizeUnityECSEntitiesWithSveltoECSEntities().RunOn(_runner); }

        protected override void Add(ref UnityECSEntityStruct             entityView,
                                    ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        {
            if (previousGroup != null && entityView.ID.groupID == GameGroups.DOOFUSESHUNGRY)
            {
                if (_uecsManager.IsCreated)
                {
                    if (_uecsManager.HasComponent(entityView.uecsEntity, typeof(UnityECSDoofusesGroup)) == false)
                        _uecsManager.AddComponent(entityView.uecsEntity, typeof(UnityECSDoofusesGroup));
                }
            }
        }

        protected override void Remove(ref UnityECSEntityStruct entityView)
        {
            if (entityView.ID.groupID == GameGroups.DOOFUSESHUNGRY)
            {
                if (_uecsManager.IsCreated)
                    _uecsManager.RemoveComponent(entityView.uecsEntity, typeof(UnityECSDoofusesGroup));
            }
        }

        IEnumerator SynchronizeUnityECSEntitiesWithSveltoECSEntities()
        {
            while (true)
            {
                var positionEntityStructs =
                    entitiesDB.QueryEntities<PositionEntityStruct>(GameGroups.DOOFUSESHUNGRY, out _);
                var positions = _group.ToComponentDataArray<Translation>(Allocator.TempJob, out var handle1);

                handle1.Complete();

                for (int index = 0; index < positions.Length; index++)
                {
                    positions[index] = new Translation
                    {
                        Value = new float3(positionEntityStructs[index].position.x,
                                           positionEntityStructs[index].position.y,
                                           positionEntityStructs[index].position.z)
                    };
                }

                //I wanted to spawn one doofus per frame, but I can't find a way to copy just a subset of entities
                _group.CopyFromComponentDataArray(positions, out var handle3);

                handle3.Complete();
                positions.Dispose();

                yield return null;
            }
        }

        public void Dispose() { _runner?.Dispose(); }

        readonly CoroutineMonoRunner _runner;
        readonly ComponentGroup      _group;
        readonly EntityManager       _uecsManager;
    }
}