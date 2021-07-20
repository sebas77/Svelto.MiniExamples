namespace Svelto.ECS.MiniExamples.Turrets
{
    public class TurretTopEntityDescriptor : ExtendibleEntityDescriptor<TransformableEntityDescriptor>
    {
        public TurretTopEntityDescriptor() : base(new IComponentBuilder[]
        {
            new ComponentBuilder<ChildComponent>()
          , new ComponentBuilder<DirectionComponent>()
        })
        { }
    }
}