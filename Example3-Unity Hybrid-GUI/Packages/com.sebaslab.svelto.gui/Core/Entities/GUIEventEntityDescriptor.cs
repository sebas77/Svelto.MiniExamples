using Svelto.DataStructures;

namespace Svelto.ECS.GUI
{
    class GUIEventEntityDescriptor : GenericEntityDescriptor<GUICustomEventComponent> { }

    struct GUICustomEventComponent : IEntityComponent
    {
        public NativeDynamicArray commandList;
        public bool               triggered;
        public StructValue        eventValue;
    }
}