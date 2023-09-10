using Svelto.ECS.Hybrid;

namespace Svelto.ECS.GUI
{
    public interface IWidgetBuilder : IWidgetInitializer
    {
        // TODO: This should include the entity factory, there is no sense in requiring builders to get it from somewhere else.
        EntityInitializer BuildWidget(IWidgetDescriptorHolder holder, IImplementor[] implementors
            , WidgetDataSource parameters);  //, IEntityFactory factory);
    }
}