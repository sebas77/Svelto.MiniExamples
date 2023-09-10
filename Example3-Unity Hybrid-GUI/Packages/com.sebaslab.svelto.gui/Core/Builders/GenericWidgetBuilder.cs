using Svelto.ECS.Hybrid;

namespace Svelto.ECS.GUI.Core.Builders
{
    public class GenericWidgetBuilder<Descriptor> : IWidgetBuilder
        where Descriptor : GUIExtendibleEntityDescriptor, new()
    {
        public GenericWidgetBuilder(IEntityFactory factory, ExclusiveBuildGroup group, IWidgetInitializer widgetInitializer = null)
        {
            _factory = factory;
            _group = group;
            _widgetInitializer = widgetInitializer;
        }

        public EntityInitializer BuildWidget(IWidgetDescriptorHolder holder, IImplementor[] implementors
            , WidgetDataSource dataSource)
        {
            return _factory.BuildEntity<Descriptor>(_nextEntityId++, _group, holder.GetImplementors());
        }

        public void Initialize(EntityInitializer initializer, IWidgetDescriptorHolder holder, WidgetDataSource data)
        {
            if (_widgetInitializer != null)
            {
                _widgetInitializer.Initialize(initializer, holder, data);
            }
        }

        readonly IEntityFactory _factory;
        readonly IWidgetInitializer _widgetInitializer;
        readonly ExclusiveBuildGroup _group;

        uint _nextEntityId;

    }
}