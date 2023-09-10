namespace Svelto.ECS.GUI
{
    public interface IGUIEntityDescriptor: IEntityDescriptor { }

    public class GUIExtendibleEntityDescriptor: ExtendibleEntityDescriptor<GUIExtendibleEntityDescriptor.GuiEntityDescriptor>, IGUIEntityDescriptor
    {
        public GUIExtendibleEntityDescriptor(IComponentBuilder[] extraEntities): base(extraEntities) { }

        public class
                GuiEntityDescriptor: GenericEntityDescriptor<GUIComponent, GUIFrameworkEventsComponent, RecyclableGUIComponent, EGIDComponent> { }
    }
}