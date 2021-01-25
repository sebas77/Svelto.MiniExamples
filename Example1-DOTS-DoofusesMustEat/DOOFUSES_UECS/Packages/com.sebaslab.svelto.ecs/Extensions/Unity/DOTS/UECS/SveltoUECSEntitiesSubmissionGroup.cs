#if UNITY_ECS
using System;
using System.Collections;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    /// <summary>
    /// Group of UECS/Svelto SystemBase engines that creates UECS entities.
    /// Svelto entities are submitted
    /// Svelto Add and remove callback are called
    /// OnUpdate of the systems are called
    /// finally the UECS command buffer is flushed
    /// Note: I cannot use Unity ComponentSystemGroups nor I can rely on the SystemBase Dependency field to
    /// solve external dependencies. External dependencies are tracked, but only linked to the UECS components operations
    /// With Dependency I cannot guarantee that an external container is used before previous jobs working on it are completed
    /// </summary>
    public sealed class SveltoUECSEntitiesSubmissionGroup
    {
        public SveltoUECSEntitiesSubmissionGroup(SimpleEntitiesSubmissionScheduler submissionScheduler, World UECSWorld)
        {
            _submissionScheduler     = submissionScheduler;
            _ECBSystem               = UECSWorld.CreateSystem<SubmissionEntitiesCommandBufferSystem>();
            _engines                 = new FasterList<SubmissionEngine>();
            _afterSubmissionEngines  = new FasterList<IUpdateAfterSubmission>();
            _beforeSubmissionEngines = new FasterList<IUpdateBeforeSubmission>();
        }

        public void SubmitEntities(JobHandle jobHandle)
        {
            if (_submissionScheduler.paused)
                return;

            using (var profiler = new PlatformProfiler("SveltoUECSEntitiesSubmissionGroup - PreSubmissionPhase"))
            {
                PreSubmissionPhase(ref jobHandle, profiler);

                //Submit Svelto Entities, calls Add/Remove/MoveTo that can be used by the IUECSSubmissionEngines
                using (profiler.Sample("Submit svelto entities"))
                {
                    _submissionScheduler.SubmitEntities();
                }

                AfterSubmissionPhase(profiler);
            }
        }

        public IEnumerator SubmitEntitiesAsync(JobHandle jobHandle, uint maxEntities)
        {
            if (_submissionScheduler.paused)
                yield break;
            
            using (var profiler = new PlatformProfiler("SveltoUECSEntitiesSubmissionGroup - PreSubmissionPhase"))
            {
                PreSubmissionPhase(ref jobHandle, profiler);

                //Submit Svelto Entities, calls Add/Remove/MoveTo that can be used by the IUECSSubmissionEngines
                while (true)
                {
                    var  submitEntitiesAsync = _submissionScheduler.SubmitEntitiesAsync(maxEntities);
                    bool moveNext;
                    using (profiler.Sample("Submit svelto entities async"))
                    {
                        moveNext = submitEntitiesAsync.MoveNext();
                    }

                    if (moveNext)
                        yield return null;
                    else
                        break;
                }

                AfterSubmissionPhase(profiler);
            }
        }

        void PreSubmissionPhase(ref JobHandle jobHandle, PlatformProfiler profiler)
        {
            JobHandle BeforeECBFlushEngines()
            {
                JobHandle jobHandle = default;

                //execute submission engines and complete jobs because of this I don't need to do _ECBSystem.AddJobHandleForProducer(Dependency);

                for (var index = 0; index < _beforeSubmissionEngines.count; index++)
                {
                    ref var engine = ref _beforeSubmissionEngines[index];
                    using (profiler.Sample(engine.name))
                    {
                        jobHandle = JobHandle.CombineDependencies(
                            jobHandle, engine.BeforeSubmissionUpdate(jobHandle));
                    }
                }

                return jobHandle;
            }

            using (profiler.Sample("Complete All Pending Jobs"))
            {
                jobHandle.Complete();
            }

            //prepare the entity command buffer to be used by the registered engines
            var entityCommandBuffer = _ECBSystem.CreateCommandBuffer();

            foreach (var system in _engines)
            {
                system.ECB = entityCommandBuffer;
            }

            using (profiler.Sample("Before Submission Engines"))
            {
                BeforeECBFlushEngines().Complete();
            }
        }

        void AfterSubmissionPhase(PlatformProfiler profiler)
        {
            JobHandle AfterECBFlushEngines()
            {
                JobHandle jobHandle = default;

                //execute submission engines and complete jobs because of this I don't need to do _ECBSystem.AddJobHandleForProducer(Dependency);
                for (var index = 0; index < _afterSubmissionEngines.count; index++)
                {
                    ref var engine = ref _afterSubmissionEngines[index];
                    using (profiler.Sample(engine.name))
                    {
                        jobHandle = JobHandle.CombineDependencies(
                            jobHandle, engine.AfterSubmissionUpdate(jobHandle));
                    }
                }

                return jobHandle;
            }

            using (profiler.Sample("Flush Command Buffer"))
            {
                _ECBSystem.Update();
            }

            using (profiler.Sample("After Submission Engines"))
            {
                AfterECBFlushEngines().Complete();
            }
        }

        public void Add(SubmissionEngine engine)
        {
            _ECBSystem.World.AddSystem(engine);
            if (engine is IUpdateAfterSubmission afterSubmission)
                _afterSubmissionEngines.Add(afterSubmission);
            if (engine is IUpdateBeforeSubmission beforeSubmission)
                _beforeSubmissionEngines.Add(beforeSubmission);
            _engines.Add(engine);
        }

        readonly SimpleEntitiesSubmissionScheduler     _submissionScheduler;
        readonly SubmissionEntitiesCommandBufferSystem _ECBSystem;
        readonly FasterList<SubmissionEngine>          _engines;
        readonly FasterList<IUpdateBeforeSubmission>   _beforeSubmissionEngines;
        readonly FasterList<IUpdateAfterSubmission>    _afterSubmissionEngines;

        [DisableAutoCreation]
        class SubmissionEntitiesCommandBufferSystem : EntityCommandBufferSystem { }
    }
}
#endif