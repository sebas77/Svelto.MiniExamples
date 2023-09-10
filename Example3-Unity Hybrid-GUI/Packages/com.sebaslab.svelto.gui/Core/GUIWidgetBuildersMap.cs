using System;
using System.Collections.Generic;

namespace Svelto.ECS.GUI
{
    public class GUIWidgetBuildersMap : IGUIWidgetBuildersReader
    {
        internal GUIWidgetBuildersMap()
        {
            _widgetBuilders = new Dictionary<Type, IWidgetBuilder>();
        }

        public void Register(Type widgetType, IWidgetBuilder builder)
        {
            _widgetBuilders.Add(widgetType, builder);
        }

        public bool TryGet(Type widgetType, out IWidgetBuilder builder)
        {
            return _widgetBuilders.TryGetValue(widgetType, out builder);
        }

        readonly Dictionary<Type, IWidgetBuilder> _widgetBuilders;
    }
}