namespace Svelto.ECS.GUI.Extensions.Unity
{
    public abstract class GenericWidgetDescriptorHolder<TDescriptor> : BaseWidgetDescriptorHolder
        where TDescriptor : IGUIEntityDescriptor, new()
    {
        public override IEntityDescriptor GetDescriptor() { return EntityDescriptorTemplate<TDescriptor>.descriptor; }
    }
}