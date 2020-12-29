namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public class PrimitiveEntityWithParentDescriptor : ExtendibleEntityDescriptor<PrimitiveEntityDescriptor>
    {
        public PrimitiveEntityWithParentDescriptor() : base(new IComponentBuilder[]
        {
            new ComponentBuilder<ObjectParentComponent>()
        }) {}
    }
}