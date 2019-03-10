using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EGIDMapper<T> where T : IEntityStruct
    {
        internal TypeSafeDictionary<T> map;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(EGID id)
        {
            int count;
            var index = map.FindElementIndex(id.entityID);
            return ref map.GetValuesArray(out count)[index];
        }
    }
}