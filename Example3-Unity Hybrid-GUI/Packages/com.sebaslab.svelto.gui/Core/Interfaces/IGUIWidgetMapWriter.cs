using System.Collections.Generic;

namespace Svelto.ECS.GUI
{
    public interface IGUIWidgetMapWriter : IGUIWidgetMapReader
    {
        void SetWidgetUniqueId(IWidgetDescriptorHolder widgetHolder, IWidgetIDHolder idHolder);

        void AddWidgetMap(string rootName, Dictionary<string, IWidgetDescriptorHolder> map);

        void RemoveWidgetMap(string rootName);
    }
}