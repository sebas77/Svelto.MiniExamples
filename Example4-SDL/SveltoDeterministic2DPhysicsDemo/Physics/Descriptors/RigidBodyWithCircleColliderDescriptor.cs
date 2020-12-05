using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents;

namespace SveltoDeterministic2DPhysicsDemo.Physics.Descriptors
{
    public class RigidBodyWithCircleColliderDescriptor : ExtendibleEntityDescriptor<RigidBodyDescriptor>
    {
        public RigidBodyWithCircleColliderDescriptor() : base(new IComponentBuilder[]
        {
            new ComponentBuilder<CircleColliderEntityComponent>()
        }) { }
    }
}