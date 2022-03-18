using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Descriptors
{
    /// <summary>
    /// RigidBody EntityDescriptor, designed to be extended
    /// </summary>
    public class RigidBodyDescriptor : GenericEntityDescriptor<TransformEntityComponent, RigidbodyEntityComponent> { }
}