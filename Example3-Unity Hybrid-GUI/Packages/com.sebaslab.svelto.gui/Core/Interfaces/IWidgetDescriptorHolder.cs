using Svelto.ECS.GUI.Commands;
using Svelto.ECS.Hybrid;

namespace Svelto.ECS.GUI
{
    public interface IWidgetDescriptorHolder
    {
        EntityReference entityReference { get; set; }

        IEntityDescriptor GetDescriptor();

        // TODO: check if both methods are still needed.
        IImplementor[] GetImplementors();

        IGUIImplementor[] GetGUIImplementors();

        T GetImplementor<T>() where T : IImplementor;

        string GetName();

        SerializedCommandData[] GetInitEventCommands(WidgetDataSource dataSource);

        SerializedCommandData[] GetDataEventCommands(WidgetDataSource dataSource);
    }
}