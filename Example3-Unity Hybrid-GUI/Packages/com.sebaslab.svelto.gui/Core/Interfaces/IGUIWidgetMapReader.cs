using System.Collections.Generic;

namespace Svelto.ECS.GUI
{
    public interface IGUIWidgetMapReader
    {
        IEnumerable<IWidgetDescriptorHolder> GetWidgetMap(string rootName);
        IEnumerable<IWidgetDescriptorHolder> GetWidgetMap(GUIComponent widgetComponent);

        IWidgetDescriptorHolder GetWidget(GUIComponent widgetComponent);

        IWidgetDescriptorHolder GetWidget(string fullName);

        IWidgetDescriptorHolder GetWidget(string rootName, string widgetName);

        IWidgetDescriptorHolder GetWidgetFromTag(GUIComponent gui, string identifier);

        IWidgetDescriptorHolder TryGetWidget(string fullName);

        IWidgetDescriptorHolder TryGetWidget(string rootName, string widgetName);

        EntityReference GetWidgetReference(string fullname);

        EntityReference GetWidgetReference(string rootName, string widgetName);

        EntityReference GetWidgetReference(uint id);

        EntityReference[] GetContainerWidgets(string container);
    }
}