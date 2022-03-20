namespace Svelto.ECS.MiniExamples.Turrets.EnemyLayer
{
    public class TurretTopEntityDescriptor : ExtendibleEntityDescriptor<TransformableEntityDescriptor>
    {
        public TurretTopEntityDescriptor() : base(new IComponentBuilder[]
        {
            new ComponentBuilder<ChildComponent>()
          , new ComponentBuilder<LookAtComponent>()
          , new ComponentBuilder<ShootingComponent>()
        })
        { }
    }
}