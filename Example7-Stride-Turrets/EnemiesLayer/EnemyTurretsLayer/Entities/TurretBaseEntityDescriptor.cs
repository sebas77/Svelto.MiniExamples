namespace Svelto.ECS.MiniExamples.Turrets.EnemyLayer
{
    public class TurretBaseEntityDescriptor : ExtendibleEntityDescriptor<TransformableEntityDescriptor>
    {
        public TurretBaseEntityDescriptor()
        {
            Add<StartPositionsComponent>();
        }
    }
}