using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Descriptors
{
        public class RigidBodyWithBoxColliderDescriptor : ExtendibleEntityDescriptor<RigidBodyDescriptor>
        {
            public RigidBodyWithBoxColliderDescriptor() : base(new IComponentBuilder[]
            {
                new ComponentBuilder<BoxColliderEntityComponent>()
            }) { }
        }
}