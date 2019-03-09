using System;
using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Svelto.Tasks.ExtraLean.Unity;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1
{
    [DisableAutoCreation]
    public class RenderingDataSynchronizationEngine: ComponentSystem, IDisposable, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }
        
        public RenderingDataSynchronizationEngine()
        {
            _positions = new NativeArray<Translation>(10000, Allocator.Persistent);
            _rotations = new NativeArray<Rotation>(10000, Allocator.Persistent);
            _runner = new CoroutineMonoRunner("test");
        }
        
        public void Ready()
        {
            SynchronizeUnityECSEntitiesWithSveltoECSEntities().RunOn(_runner);
        }

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            
            m_Group = GetComponentGroup(typeof(Translation), typeof(Rotation));
        }

        IEnumerator SynchronizeUnityECSEntitiesWithSveltoECSEntities()
        {
            while (m_Group == null) yield return null;

                while (true)
                {
                    var entities =
                        entitiesDB
                           .QueryEntities<RotationEntityStruct, PositionEntityStruct>(GameGroups.DOOFUSES,
                                                                                      out _);

                    for (int index = 0; index != _positions.Length; index++)
                    {
                        _positions[index] = new Translation
                        {
                            Value = new float3(entities.Item2[index].position.x,
                                               entities.Item2[index].position.y,
                                               entities.Item2[index].position.z)
                        };
                        _rotations[index] = new Rotation
                        {
                            Value = new quaternion(entities.Item1[index].rotation.x,
                                                   entities.Item1[index].rotation.y,
                                                   entities.Item1[index].rotation.z,
                                                   entities.Item1[index].rotation.w)
                        };
                    }

                    m_Group.CopyFromComponentDataArray(_positions, out var handle1);
                    m_Group.CopyFromComponentDataArray(_rotations, out var handle2);
                    
                    JobHandle.CombineDependencies(handle1, handle2).Complete();

                    yield return null;
                }
            
        }
        
        public void Dispose()
        {
            _positions.Dispose();
            _rotations.Dispose();

            _runner?.Dispose();
        }
        
        protected override void OnUpdate()
        {}

        readonly CoroutineMonoRunner _runner;
        ComponentGroup m_Group;
        NativeArray<Translation> _positions;
        NativeArray<Rotation> _rotations;
    }
}
