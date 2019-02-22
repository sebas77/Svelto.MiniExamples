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
            _runner = new UpdateMonoRunner("test");
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

        UpdateMonoRunner _runner;
        ComponentGroup m_Group;
    }
}


#if ignore
 public class RenderingDataSyncronizationEngine:ComponentSystem, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }
        
        public RenderingDataSyncronizationEngine(ThreadSynchronizationSignal synchronizationSignal)
        {
            _synchronizationSignal = synchronizationSignal;
        }

        public void Ready()
        {
            _ready = true;
        }
        
        protected override void OnUpdate()
        {
            if (_ready == false || _synchronizationSignal.Wait().MoveNext() == true) return;

            int count;
            var entities = entitiesDB.QueryEntities<BoidEntityStruct>
                (GAME_GROUPS.BOIDS_GROUP, out count);

            int index = 0;
            
            ForEach( (ref Position position, ref Rotation rotation) =>
            {
                position = new Position {Value = new float3(entities[index].position.x, entities[index].position.y, 
                                                            entities[index].position.z)};
                rotation = new Rotation { Value = new quaternion(entities[index].rotation.x, entities[index].rotation.y, 
                                                            entities[index].rotation.z, entities[index].rotation.w)};
            }, GetComponentGroup(typeof(Position), typeof(Rotation)));
            
            _synchronizationSignal.SignalBack();
        }

        readonly ThreadSynchronizationSignal _synchronizationSignal;
        bool _ready;
    }
#endif