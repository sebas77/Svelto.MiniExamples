using System;
using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI
{
    public interface IGUIBuilder : IDisposable
    {
        IGUIWidgetBuildersReader buildersMap { set; }
        IGUIWidgetMapWriter      widgetMap   { set; }
        GUIResources             resources   { set; }
        GUIBlackboard            blackboard  { set; }

        BuildWidgetEntityFunc    BuildWidgetEntity { set; }

        delegate EntityReference BuildWidgetEntityFunc(string widgetName, string containerFullname
            , WidgetDataSource dataSource);

        WidgetDefinition[] GetStaticWidgets();

        void CreateInstance(string widgetName, IWidgetDescriptorHolder container, out WidgetDefinition widget
            , out RecyclableGUIComponent recyclable);

        void FreeInstance(in RecyclableGUIComponent recyclable);
    }

    public struct WidgetDefinition
    {
        public IWidgetDescriptorHolder   root;
        public IWidgetDescriptorHolder[] childWidgets;
    }
}