using Svelto.ECS;
using MiniExamples.DeterministicPhysicDemo.Physics.EntityComponents;

namespace MiniExamples.DeterministicPhysicDemo.Physics.Descriptors
{
    public class CollisionDescriptor : GenericEntityDescriptor<CollisionManifoldEntityComponent, ImpulseEntityComponent, ReferenceEgidComponent, EGIDComponent> { }
}