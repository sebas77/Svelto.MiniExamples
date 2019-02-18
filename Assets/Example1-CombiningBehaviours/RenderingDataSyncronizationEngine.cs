using System;
using System.Collections;
using Svelto.Tasks.ExtraLean;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class RenderingDataSyncronizationEngine: IQueryingEntitiesEngine, IDisposable
    {
        public IEntitiesDB entitiesDB { get; set; }
        
        public RenderingDataSyncronizationEngine(ThreadSynchronizationSignal synchronizationSignal)
        {
            _synchronizationSignal = synchronizationSignal;
            _runner = new UnityJobRunner("test");
        }
        
        public void Ready()
        {
            SynchronizeUnityECSEntitiesWithSveltoECSEntities().RunOn(_runner);
        }

        IEnumerator SynchronizeUnityECSEntitiesWithSveltoECSEntities()
        {
            while (true)
            {
                //this is the normal not blocking operation. The frame rate will be as fast as the unity main thread
                //can be, but the boids will still update at the real execution rate
                yield return _synchronizationSignal;
                
                int count;
                //fetch the Svelto.ECS entities
                var entities = entitiesDB.QueryEntities<BoidEntityStruct>
                               (GAME_GROUPS.BOIDS_GROUP, out count);
                //fetch the Unity ECS components
           //     var position = _unityECSgroup.GetComponentDataArray<Position>();
             //   var rotation = _unityECSgroup.GetComponentDataArray<Rotation>();
                
                //synchronize!
                for (int i = 0; i < count; ++i)
                {
           //         position[i] = new Position()
             //           {Value = new float3(entities[i].position.x, entities[i].position.y, entities[i].position.z)};
               //     rotation[i] = new Rotation() 
                 //   { Value = new quaternion(entities[i].rotation.x, entities[i].rotation.y, entities[i].rotation.z, 
                   //                          entities[i].rotation.w)};
                }

                //tell to the Svelto.Tasks threads that they can carry on with the next iteration
                _synchronizationSignal.SignalBack();

                //yield one frame so the while (true) will not enter in an infinite loop
                yield return null;
            }
        }
        
        public void Dispose()
        {
            _runner?.Dispose();
        }

        readonly ThreadSynchronizationSignal _synchronizationSignal;
        UnityJobRunner _runner;
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