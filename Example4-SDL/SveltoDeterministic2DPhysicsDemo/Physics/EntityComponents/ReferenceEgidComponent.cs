using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents
{
    public struct ReferenceEgidComponent : IEntityComponent
    {
        public EGID Egid { get; set; }

        public ReferenceEgidComponent(EGID egid)
        {
            Egid = egid;
        }
    }
}