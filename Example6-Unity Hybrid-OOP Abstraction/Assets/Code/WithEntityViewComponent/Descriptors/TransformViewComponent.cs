using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Example.OOPAbstraction.EntityViewComponents
{
    struct TransformViewComponent : IEntityViewComponent
    {
        public ITransformImplementor transform;
        public EGID                  ID { get; set; }
    }
}