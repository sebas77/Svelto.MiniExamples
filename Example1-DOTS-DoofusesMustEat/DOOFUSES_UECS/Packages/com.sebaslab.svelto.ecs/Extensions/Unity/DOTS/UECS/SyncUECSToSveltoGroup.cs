#if UNITY_ECS
using Unity.Entities;
using Unity.Jobs;

// Forgive me I ask similar questions several times, but I need to be 1000% sure about what I am doing. If I run N SystemBase systems one after the other, can I expect the Dependency field of the last one to carry all the dependencies collected so far, or each SystemBase have a different Dependency handle where the dependencies information are not cumulative but indipentend?
// 25 replies
//
// Sebastiano Mandala  1 hour ago
// in other words, if I want to complete the ForEach jobs started by N SystemBases in a sync point, can I do that with Dependency.Complete() of the last sytem? (edited) 
//
// Sebastiano Mandala  1 hour ago
// right now we are "registering" the Dependency handle of each SystemBase in a custom static handle and I am not sure if that is needed (edited) 
//
// Sebastiano Mandala  1 hour ago
// we are basically doing this at the end of each systembase:
// DeterministicStepJobHandle.Register(Dependency);
//
// Sebastiano Mandala  1 hour ago
// and then DeterministicStepJobHandle.Complete in the sync point.
//
// Cort Stratton:spiral_calendar_pad:  32 minutes ago
// Each system maintains a list of component types that it reads and write. Things like GetComponentDataFromEntity(), GetEntityQuery(), and Entities.ForEach() implicitly register the relevant reader/writer types for the calling system. The value of a system's Dependency property at the beginning of its OnUpdate() method is the combined JobHandle of all unComplete()ed jobs scheduled by earlier systems that access the system's registered components. After OnUpdate(), the output Dependency handle is registered with the safety system against all the system's registered types, so that downstream systems that access the same types know to wait on its jobs as well.
//
// Cort Stratton:spiral_calendar_pad:  31 minutes ago
// So to get back to your question, unless there's some subtlety I'm missing, I believe it should be sufficient to only complete the input Dependency handle, as it captures all the component-related job dependencies from all previous systems. There's no need to iterate backwards and complete each previous system's Dependency manually. (edited) 
//
// Sebastiano Mandala  30 minutes ago
// thank you, just one sec
//
// Sebastiano Mandala  30 minutes ago
// our Register wasn't in fact storing an array of handles, was just combining them in one only
//
// Sebastiano Mandala  29 minutes ago
// but you are saying that the "last" dependency should have the same info right?
//
// Sebastiano Mandala  29 minutes ago
// the register was doing just this:
// public static JobHandle Register(JobHandle inputDeps)        {
//             deterministicCombinedHandles = JobHandle.CombineDependencies(deterministicCombinedHandles, inputDeps);
//
//             return deterministicCombinedHandles;
//         }
// (edited)
//
// Sebastiano Mandala  29 minutes ago
// and then we would have completed deterministicCombinedHandles
//
// Sebastiano Mandala  28 minutes ago
// i am not sure what you mean by "input dependency handle"
//
// Cort Stratton:spiral_calendar_pad:  27 minutes ago
// By "input dependency handle" I mean "the value of this.Dependency at the beginning of OnUpdate() (edited) 
//
// Sebastiano Mandala  26 minutes ago
// yes, you are confident that this can work right? also it will carry all the dependencies combined so far, not only the ones coming from ForEach
//
// Cort Stratton:spiral_calendar_pad:  26 minutes ago
// The value of this.Dependency at the beginning of OnUpdate() is the combination of the handles of the most recent jobs that read/write each component that the system has registered as a read/write dependency.
//
// Sebastiano Mandala  26 minutes ago
// yes but what if I add external dependencies like this:
//
// Sebastiano Mandala  26 minutes ago
// Dependency = JobHandle.CombineDependencies(Dependency, inputDeps);
//
// Sebastiano Mandala  26 minutes ago
// inputDeps will be carried down the line, correct?
//
// Cort Stratton:spiral_calendar_pad:  25 minutes ago
// Yes. At the end of OnUpdate(), the final Dependency value is registered as the new "latest job handle" for each of the system's registered component types.
//
// Sebastiano Mandala  24 minutes ago
// ok so I will get rid of our static combined handle and work with the "last one" executed/combined (edited) 
//
// Sebastiano Mandala  24 minutes ago
// thank you!
//
// Cort Stratton:spiral_calendar_pad:  22 minutes ago
// So (pseudocode) if SystemA.OnUpdate() sets Dependency to jobA.Schedule(inputDeps), then the safety system will associate jobA's JobHandle with all types read/written by SystemA. If SystemB has registered a read or write dependency on any of the same types, then the Dependency at the beginning of SystemB.OnUpdate() will include a dependency on jobA's handle (which, in turn, contains a dependency on the inputDeps passed when jobA was scheduled. (edited) 
//
// Sebastiano Mandala  21 minutes ago
// yes cool that was my question, I was wondering if B would carry only the dependencies from A of the component types B is working with
//
// Cort Stratton:spiral_calendar_pad:  21 minutes ago
// So yes, I'm pretty sure we are agreeing with each other :slightly_smiling_face:. That is the intent in any case; if you find a case where that isn't the behavior you're seeing, that sounds like it would be a bug; please let us know!
// :+1:
// 1
//
//
// Sebastiano Mandala  21 minutes ago
// but you are instead saying it will carry ALL the dependencies, no matter what (edited) 

namespace Svelto.ECS.Extensions.Unity
{
    public class SyncUECSToSveltoGroup : JobifiedEnginesGroup<SyncUECSToSveltoEngine>
    {
        
    }

    public abstract class SyncUECSToSveltoEngine : SystemBase, IJobifiedEngine
    {
        public JobHandle Execute(JobHandle inputDeps)
        {
            Dependency = JobHandle.CombineDependencies(Dependency, inputDeps);
            
            Update();

            return Dependency;
        }

        public abstract string name { get; }
    }
}
#endif