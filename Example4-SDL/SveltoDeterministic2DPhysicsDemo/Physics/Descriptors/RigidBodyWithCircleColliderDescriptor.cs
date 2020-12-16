using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Descriptors
{
    /// <summary>
    /// Todo: collision with circle can be added and possibly will be in future iteration of this demo
    /// </summary>
    public class RigidBodyWithCircleColliderDescriptor : ExtendibleEntityDescriptor<RigidBodyDescriptor>
    {
        public RigidBodyWithCircleColliderDescriptor() : base(new IComponentBuilder[]
        {
            new ComponentBuilder<CircleColliderEntityComponent>()
        }) { }
    }
}