using Svelto.ECS;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Unity;

namespace Boxtopia.GUIs.Generic
{
    class GuiViewSwitchButtonDescriptorHolder : GenericEntityDescriptorHolder<GuiViewSwitchButtonDescriptor>
    {
    }

    class GuiViewSwitchButtonDescriptor : GenericEntityDescriptor<GuiViewIndexEntityViewStruct, ButtonEntityViewStruct, ButtonEntityStruct>
    {
    }

    struct GuiViewIndexEntityViewStruct : IEntityViewStruct
    {
        public IGuiViewIndex guiViewIndex;

        public EGID ID { get; set; }
    }
}
