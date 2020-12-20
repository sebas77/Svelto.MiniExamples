using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics
{
    public static class GameGroups
    {
        public class RigidBodies : RigidBody { }
        public class DynamicRigidBodies : GroupCompound<RigidBody, Dynamic> { }
        public class DynamicRigidBodyWithBoxColliders : GroupCompound<RigidBody, WithBoxCollider, Dynamic> { }
        public class DynamicRigidBodyWithCircleColliders : GroupCompound<RigidBody, WithCircleCollider, Dynamic> { }
        public class KinematicRigidBodyWithBoxColliders : GroupCompound<RigidBody, WithBoxCollider, Kinematic> { }
        public class KinematicRigidBodyWithCircleColliders : GroupCompound<RigidBody, WithCircleCollider, Kinematic> { }
        
        public class RigidBody : GroupTag<RigidBody> { }
        public class Kinematic : GroupTag<Kinematic> { }
        public class Dynamic : GroupTag<Dynamic> { }
        public class WithBoxCollider : GroupTag<WithBoxCollider> { }
        public class WithCircleCollider : GroupTag<WithCircleCollider> { }
    }
}