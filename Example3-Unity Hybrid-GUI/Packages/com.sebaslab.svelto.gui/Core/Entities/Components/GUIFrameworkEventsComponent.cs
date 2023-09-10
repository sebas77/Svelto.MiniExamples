using Svelto.DataStructures;
using Svelto.DataStructures.Native;

namespace Svelto.ECS.GUI
{
    public struct GUIFrameworkEventsComponent : IEntityComponent
    {
        internal SharedSveltoDictionaryNative<int, NativeDynamicArray> map;
    }
}