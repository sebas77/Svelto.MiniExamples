namespace Svelto.ECS.GUI
{
    public interface IWidgetInitializer
    {
        void Initialize(EntityInitializer initializer, IWidgetDescriptorHolder holder, WidgetDataSource data);
    }
}