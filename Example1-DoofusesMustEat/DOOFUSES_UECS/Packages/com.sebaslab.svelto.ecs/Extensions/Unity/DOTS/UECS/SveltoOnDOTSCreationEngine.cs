using Unity.Jobs;

#if UNITY_ECS
namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    /// SubmissionEngine is a dedicated DOTS ECS Svelto.ECS engine that allows using the DOTS ECS
    /// EntityCommandBuffer for fast creation of DOTS entities
    /// </summary>
    public interface ISveltoOnDOTSStructuralEngine
    {
        public DOTSOperationsForSvelto DOTSOperations { get; set; }

        public string name { get; }
    }
}
#endif