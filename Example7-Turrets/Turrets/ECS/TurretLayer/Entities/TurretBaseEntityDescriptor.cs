namespace Svelto.ECS.MiniExamples.Turrets
{
    public class TurretBaseEntityDescriptor : ExtendibleEntityDescriptor<TransformableEntityDescriptor>
    {
        public TurretBaseEntityDescriptor()
        {
            Add<StartPositionsComponent>();
        }
    }
}