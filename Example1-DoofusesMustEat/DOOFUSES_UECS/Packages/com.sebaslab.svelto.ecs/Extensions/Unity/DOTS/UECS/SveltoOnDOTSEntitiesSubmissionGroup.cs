#if UNITY_ECS
using System;
using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Native;
using Svelto.ECS.Schedulers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Allocator = Unity.Collections.Allocator;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    ///     SveltoDOTS ECSEntitiesSubmissionGroup expand the _submissionScheduler responsibility to integrate the
    ///     submission of Svelto entities with the submission of DOTS ECS entities using EntityCommandBuffer.
    ///     As there is just one submissionScheduler per enginesRoot, there should be only one SveltoDOTS
    ///     ECSEntitiesSubmissionGroup
    ///     per engines group. It's expected use is showed in the class SveltoOnDOTS ECSEnginesGroup which should be used
    ///     instead of using this class directly.
    ///     Groups DOTS ECS/Svelto SystemBase engines that creates DOTS ECS entities.
    ///     Flow:
    ///     Complete all the jobs used as input dependencies (this is a sync point)
    ///     Create the new frame Command Buffer to use
    ///     Svelto entities are submitted
    ///     Svelto Add and remove callback are called
    ///     ECB is injected in all the registered engines
    ///     all the OnUpdate of the registered engines/systems are called
    ///     the DOTS ECS command buffer is flushed
    ///     all the DOTS ECS entities created that need Svelto information will be processed
    /// </summary>
    [DisableAutoCreation]
    public sealed partial class SveltoOnDOTSEntitiesSubmissionGroup: SystemBase, IQueryingEntitiesEngine,
            ISveltoOnDOTSSubmission
    {
        public SveltoOnDOTSEntitiesSubmissionGroup(SimpleEntitiesSubmissionScheduler submissionScheduler)
        {
            _submissionScheduler = submissionScheduler;
            _submissionEngines = new FasterList<SveltoOnDOTSHandleStructuralChangesEngine>();
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        //Right, when you record a command outside of a job using the regular ECB, you don't pass it a sort key.
        //We instead use a constant for the main thread that is actually set to Int32.MaxValue. Where as the commands
        //that are recording from jobs with the ParallelWriter, get a lower value sort key from the job. Because we
        //playback the commands in order based on this sort key, the ParallelWriter commands end up happening before
        //the main thread commands. This is where your error is coming from because the Instantiate command happens at
        //the end because it's sort key is Int32.MaxValue.
        //We don't recommend mixing the main thread and ParallelWriter commands in a single ECB for this reason.
        public void SubmitEntities(JobHandle jobHandle)
        {
            if (_submissionScheduler.paused == true)
                return;

            using (var profiler = new PlatformProfiler("SveltoDOTSEntitiesSubmissionGroup"))
            {
                using (profiler.Sample("PreSubmissionPhase"))
                {
                    PreSubmissionPhase(ref jobHandle, profiler);
                }

                //Submit Svelto Entities, calls Add/Remove/MoveTo that can be used by the DOTS ECSSubmissionEngines
                _submissionScheduler.SubmitEntities();

                using (profiler.Sample("AfterSubmissionPhase"))
                {
                    AfterSubmissionPhase(profiler);
                }
            }
        }

        public void Add(SveltoOnDOTSHandleStructuralChangesEngine engine)
        {
            // Console.LogDebug($"Add Submission Engine {engine} to the DOTS world {_ECBSystem.World.Name}");

            //this is temporary enabled because of engines that needs EntityManagers for the wrong reasons.
            _submissionEngines.Add(engine);
            engine.OnCreate();
        }

        void PreSubmissionPhase(ref JobHandle jobHandle, PlatformProfiler profiler)
        {
            using (profiler.Sample("Complete All Pending Jobs"))
            {
                jobHandle.Complete(); //sync-point
                EntityManager.CompleteAllTrackedJobs();
            }

            foreach (var system in _submissionEngines)
            {
                system.DOTSOperations = new DOTSBatchedOperationsForSvelto(World.EntityManager);
            }
        }

        void AfterSubmissionPhase(PlatformProfiler profiler)
        {
            JobHandle combinedHandle = default;

            using (profiler.Sample("Run submission engines"))
            {
                for (var i = 0; i < _submissionEngines.count; i++)
                {
                    try
                    {
                        combinedHandle = JobHandle.CombineDependencies(combinedHandle, _submissionEngines[i].OnPostSubmission());
                    }
                    catch (Exception e)
                    {
                        Console.LogException(e, _submissionEngines[i].name);

                        throw;
                    }
                }
            }
            
            combinedHandle.Complete();
            
            for (var i = 0; i < _submissionEngines.count; i++)
            {
                try
                {
                   _submissionEngines[i].CleanUp();
                }
                catch (Exception e)
                {
                    Console.LogException(e, _submissionEngines[i].name);

                    throw;
                }
            }
        }

        protected override void OnCreate()
        {
        }

        protected override void OnUpdate()
        {
            throw new NotSupportedException("if this is called something broke the original design");
        }

        readonly FasterList<SveltoOnDOTSHandleStructuralChangesEngine> _submissionEngines;

        readonly SimpleEntitiesSubmissionScheduler _submissionScheduler;
    }
}
#endif