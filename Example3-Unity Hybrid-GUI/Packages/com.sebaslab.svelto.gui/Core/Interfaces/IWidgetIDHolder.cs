
namespace Svelto.ECS.GUI
{
    /**
     * Widget ID Holder is an interface provided to allow engines to query the widget map for a specific widgets using
     * a unique ID. It is the responsability of the platform GUI builder to registering widgets that need a unique id
     * by calling the IGUIWidgetMapWriter.SetWidgetUniqueId(IWidgetDescriptorHolder, IWidgetIDHolder).
     * Interested user code can then query this widgets by calling IGUIWidgetMapReader.GetWidget(uint id).
     */
    public interface IWidgetIDHolder
    {
        uint WidgetID { get; set; }
    }
}