using Svelto.ECS.EntityStructs;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1B
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class RenderingDataSynchronizationEngine : ComponentSystem, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        protected override void OnUpdate()
        {
            if (entitiesDB == null) return;
            
            var positionEntityStructs =
                entitiesDB.QueryEntities<PositionEntityStruct>(GameGroups.DOOFUSES, out _);

            int index = 0;

            Entities.WithAll<Translation, UnityECSDoofusesGroup>().ForEach((ref Translation translation) =>
                                                                           {
                                                                               ref var positionEntityStruct = ref positionEntityStructs[index++];

                                                                               translation.Value = new float3(positionEntityStruct.position.x,
                                                                                                              positionEntityStruct.position.y,
                                                                                                              positionEntityStruct.position.z);
                                                                           });
        }

        public void Ready() { }
    }
}