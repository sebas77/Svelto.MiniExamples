using Svelto.ECS;

namespace SveltoDeterministic2DPhysicsDemo.Physics
{
    public static class GameGroups
    {
        public class RigidBodies : RigidBodyTag { }
        public class RigidBodyWithBoxColliders : GroupCompound<RigidBodyTag, RigidBodyWithBoxColliderTag> { }
        public class RigidBodyWithCircleColliders : GroupCompound<RigidBodyTag, RigidBodyWithCircleColliderTag> { }
        
        public class RigidBodyTag : GroupTag<RigidBodyTag> { }
        public class RigidBodyWithBoxColliderTag : GroupTag<RigidBodyWithBoxColliderTag> { }
        public class RigidBodyWithCircleColliderTag : GroupTag<RigidBodyWithCircleColliderTag> { }
    }
}