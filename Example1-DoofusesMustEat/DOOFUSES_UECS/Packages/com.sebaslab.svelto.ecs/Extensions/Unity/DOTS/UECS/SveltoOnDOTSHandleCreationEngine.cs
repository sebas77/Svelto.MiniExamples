#if UNITY_ECS
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// SubmissionEngine is a dedicated DOTS ECS Svelto.ECS engine that allows using the DOTS ECS
    /// EntityCommandBuffer for fast creation of DOTS entities
    /// </summary>
    public abstract class SveltoOnDOTSHandleStructuralChangesEngine
    {
        protected internal DOTSBatchedOperationsForSvelto DOTSOperations;

        protected internal virtual void OnCreate()
        {
        }

        protected internal virtual JobHandle OnPostSubmission()
        {
            return default;
        }

        protected internal virtual void CleanUp()
        {
            
        }

        public abstract string name { get; }
        
    }
}
#endif