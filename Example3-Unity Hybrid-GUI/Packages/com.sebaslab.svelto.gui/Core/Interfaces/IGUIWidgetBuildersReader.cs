using System;

namespace Svelto.ECS.GUI
{
    public interface IGUIWidgetBuildersReader
    {
        bool TryGet(Type widgetType, out IWidgetBuilder builder);
    }
}