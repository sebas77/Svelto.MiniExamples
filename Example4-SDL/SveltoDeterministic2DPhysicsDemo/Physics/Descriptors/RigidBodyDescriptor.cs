using Svelto.ECS;
using SveltoDeterministic2DPhysicsDemo.Physics.EntityComponents;

namespace SveltoDeterministic2DPhysicsDemo.Physics.Descriptors
{
    public class RigidBodyDescriptor : GenericEntityDescriptor<TransformEntityComponent, RigidbodyEntityComponent,
        CollisionManifoldEntityComponent, EGIDComponent> { }
}