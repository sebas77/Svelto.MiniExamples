using System;
using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Svelto.Tasks.ExtraLean.Unity;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1
{
    [DisableAutoCreation]
    public class RenderingDataSyncronizationEngine: ComponentSystem, IDisposable, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }
        
        public RenderingDataSyncronizationEngine()
        {
            _runner = new CoroutineMonoRunner("test");
        }
        
        public void Ready()
        {
            SynchronizeUnityECSEntitiesWithSveltoECSEntities().RunOn(_runner);
        }

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            
            m_Group = GetComponentGroup(typeof(Position), typeof(Rotation));
        }

        IEnumerator SynchronizeUnityECSEntitiesWithSveltoECSEntities()
        {
            while (m_Group == null) yield return null;
            
            while (true)
            {
                var entities =
                    entitiesDB.QueryEntities<RotationEntityStruct, PositionEntityStruct>(GameGroups.DOOFUSES,
                                                                                         out var count);
                    
                var positions = m_Group.GetComponentDataArray<Position>();
                var rotations = m_Group.GetComponentDataArray<Rotation>();

                if (count != positions.Length)
                {
                    yield return null;
                    
                    continue;
                }

                for (int index = 0; index != positions.Length; index++)
                {
                    positions[index] = new Position
                    {
                        Value = new float3(entities.Item2[index].position.x, entities.Item2[index].position.y,
                                           entities.Item2[index].position.z)
                    };
                    rotations[index] = new Rotation
                    {
                        Value = new quaternion(entities.Item1[index].rotation.x, entities.Item1[index].rotation.y,
                                               entities.Item1[index].rotation.z, entities.Item1[index].rotation.w)
                    };
                }

                yield return null;
            }
        }
        
        public void Dispose()
        {
            _runner?.Dispose();
        }
        
        protected override void OnUpdate()
        {}

        readonly CoroutineMonoRunner _runner;
        ComponentGroup m_Group;
    }
}
