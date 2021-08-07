namespace Svelto.ECS.MiniExamples.Turrets
{
    public class BulletEntityDescriptor : ExtendibleEntityDescriptor<TransformableEntityDescriptor> 
    {
        public BulletEntityDescriptor()
        {
            Add<BulletComponent>();
        }
    }
}