using Svelto.ECS;

namespace MiniExamples.DeterministicPhysicDemo.Physics
{
    public static class GameGroups
    {
        public abstract class RigidBodies : GroupTag<RigidBodies> { }
        public abstract class Kinematic : GroupTag<Kinematic> { }
        public abstract class Dynamic : GroupTag<Dynamic> { }
        public abstract class WithBoxCollider : GroupTag<WithBoxCollider> { }
        public abstract class WithCircleCollider : GroupTag<WithCircleCollider> { }
        
        public abstract class DynamicRigidBodies : GroupCompound<RigidBodies, Dynamic> { }
        public abstract class DynamicRigidBodyWithBoxColliders : GroupCompound<RigidBodies, WithBoxCollider, Dynamic> { }
        public abstract class DynamicRigidBodyWithCircleColliders : GroupCompound<RigidBodies, WithCircleCollider, Dynamic> { }
        public abstract class KinematicRigidBodyWithBoxColliders : GroupCompound<RigidBodies, WithBoxCollider, Kinematic> { }
        public abstract class KinematicRigidBodyWithCircleColliders : GroupCompound<RigidBodies, WithCircleCollider, Kinematic> { }
    }
}