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
    public class RenderingDataSynchronizationEngine : IDisposable, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public RenderingDataSynchronizationEngine(World world)
        {
            _runner = new CoroutineMonoRunner("test");
            _group = world.EntityManager.CreateComponentGroup(typeof(Translation),
                                                              typeof(UnityECSDoofusesGroup));
        }

        public void Ready()
        {
            SynchronizeUnityECSEntitiesWithSveltoECSEntities().RunOn(_runner);
        }

        IEnumerator SynchronizeUnityECSEntitiesWithSveltoECSEntities()
        {
            while (true)
            {
                var positionEntityStructs =
                    entitiesDB
                       .QueryEntities<PositionEntityStruct>(GameGroups.DOOFUSES,
                                                                                  out _);
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

        public void Dispose()
        {
            _runner?.Dispose();
        }

        readonly CoroutineMonoRunner _runner;
        ComponentGroup               _group;
    }
}