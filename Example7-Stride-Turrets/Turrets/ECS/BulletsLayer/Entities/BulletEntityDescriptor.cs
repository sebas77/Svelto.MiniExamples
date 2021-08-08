namespace Svelto.ECS.MiniExamples.Turrets
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