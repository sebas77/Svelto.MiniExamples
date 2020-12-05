using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents;

namespace SveltoDeterministic2DPhysicsDemo.Physics.Descriptors
{
        public class RigidBodyWithBoxColliderDescriptor : ExtendibleEntityDescriptor<RigidBodyDescriptor>
        {
            public RigidBodyWithBoxColliderDescriptor() : base(new IComponentBuilder[]
            {
                new ComponentBuilder<BoxColliderEntityComponent>()
            }) { }
        }
}