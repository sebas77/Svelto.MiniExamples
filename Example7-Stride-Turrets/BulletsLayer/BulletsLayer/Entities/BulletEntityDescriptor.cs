using Svelto.ECS.MiniExamples.Turrets.PhysicLayer;

namespace Svelto.ECS.MiniExamples.Turrets.BulletLayer
{
    public class BulletEntityDescriptor : ExtendibleEntityDescriptor<TransformableEntityDescriptor> 
    {
        public BulletEntityDescriptor()
        {
            ExtendWith<PhysicEntityDescriptor>();
            Add<BulletComponent>();
        }
    }
}