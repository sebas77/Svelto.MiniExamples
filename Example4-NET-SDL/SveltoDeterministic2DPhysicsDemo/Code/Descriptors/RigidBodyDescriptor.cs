using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Descriptors
{
    /// <summary>
    /// RigidBody EntityDescriptor, designed to be exteded
    /// </summary>
    public class RigidBodyDescriptor : GenericEntityDescriptor<TransformEntityComponent, RigidbodyEntityComponent, EGIDComponent> { }
}