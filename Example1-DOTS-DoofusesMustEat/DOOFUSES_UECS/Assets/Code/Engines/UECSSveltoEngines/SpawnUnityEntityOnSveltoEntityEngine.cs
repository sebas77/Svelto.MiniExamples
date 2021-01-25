using Svelto.ECS.Extensions.Unity;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1C
{
    /// <summary>
    /// In a Svelto<->UECS scenario, is common to have UECS entity created on creation of Svelto ones. Same for
    /// destruction.
    /// Note this can be easily moved to using Entity Command Buffer and I should do it at a given point
    /// </summary>
    [DisableAutoCreation]
    public class SpawnUnityEntityOnSveltoEntityEngine : SubmissionEngine, IQueryingEntitiesEngine
                                                      , IUpdateAfterSubmission, IUpdateBeforeSubmission
                                                      , IReactOnAddAndRemove<UECSEntityComponent>
                                                      , IReactOnSwap<UECSEntityComponent>
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { _entityQuery = GetEntityQuery(typeof(UpdateUECSEntityAfterSubmission)); }

        public void Add(ref UECSEntityComponent entityComponent, EGID egid)
        {
            Entity uecsEntity = ECB.Instantiate(entityComponent.uecsEntity);

            //SharedComponentData can be used to group the UECS entities exactly like the Svelto ones
            ECB.AddSharedComponent(uecsEntity, new UECSSveltoGroupID(egid.groupID));
            ECB.AddComponent(uecsEntity, new UpdateUECSEntityAfterSubmission(egid));
            ECB.SetComponent(uecsEntity, new Translation
            {
                Value = new float3(entityComponent.spawnPosition.x, entityComponent.spawnPosition.y
                                 , entityComponent.spawnPosition.z)
            });
        }

        public void Remove(ref UECSEntityComponent entityComponent, EGID egid)
        {
            ECB.DestroyEntity(entityComponent.uecsEntity);
        }

        public void MovedTo(ref UECSEntityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            ECB.SetSharedComponent(entityComponent.uecsEntity, new UECSSveltoGroupID(egid.groupID));
        }

        public JobHandle BeforeSubmissionUpdate(JobHandle jobHandle)
        {
             Dependency = JobHandle.CombineDependencies(jobHandle, Dependency);
            
             ECB.RemoveComponentForEntityQuery<UpdateUECSEntityAfterSubmission>(_entityQuery);
            
             return Dependency;
        }

        public JobHandle AfterSubmissionUpdate(JobHandle jobHandle)
        {
            if (_entityQuery.IsEmpty == false)
            {
                NativeEGIDMultiMapper<UECSEntityComponent> mapper =
                    entitiesDB.QueryNativeMappedEntities<UECSEntityComponent>(
                        entitiesDB.FindGroups<UECSEntityComponent>());

                Dependency = JobHandle.CombineDependencies(jobHandle, Dependency);

                Entities.ForEach((Entity id, ref UpdateUECSEntityAfterSubmission egidComponent) =>
                {
                    mapper.Entity(egidComponent.egid).uecsEntity = id;
                }).ScheduleParallel();

                mapper.ScheduleDispose(jobHandle);

                return Dependency;
            }

            return jobHandle;
        }

        EntityQuery _entityQuery;
    }
}