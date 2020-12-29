namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public class PrimitiveEntityDescriptorWithParent : ExtendibleEntityDescriptor<PrimitiveEntityDescriptor>
    {
        public PrimitiveEntityDescriptorWithParent() : base(new IComponentBuilder[]
        {
            new ComponentBuilder<ObjectParentComponent>()
        }) {}
    }
}