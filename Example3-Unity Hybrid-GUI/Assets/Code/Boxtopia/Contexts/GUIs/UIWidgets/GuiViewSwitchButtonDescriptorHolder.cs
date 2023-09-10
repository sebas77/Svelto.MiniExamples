using Svelto.ECS;
using Svelto.ECS.Extensions.Unity;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Hybrid.Unity;

namespace Boxtopia.GUIs.Generic
{
    class GuiViewSwitchButtonDescriptorHolder : GenericEntityDescriptorHolder<GuiViewSwitchButtonDescriptor>
    {
    }

    class GuiViewSwitchButtonDescriptor : GenericEntityDescriptor<GuiViewIndexEntityViewComponent, ButtonEntityViewComponent, ButtonEntityComponent>
    {
    }

    struct GuiViewIndexEntityViewComponent : IEntityViewComponent
    {
        public IGuiViewIndex guiViewIndex;

        public EGID ID { get; set; }
    }
}
