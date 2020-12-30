namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    /// <summary>
    /// this specialised the PrimitiveEntityDescriptor to give to the entity the option to be parented to
    /// a parent object.
    /// </summary>
    public class PrimitiveEntityWithParentDescriptor : ExtendibleEntityDescriptor<PrimitiveEntityDescriptor>
    {
        public PrimitiveEntityWithParentDescriptor() : base(new IComponentBuilder[]
        {
            new ComponentBuilder<ObjectParentComponent>()
        }) {}
    }
}