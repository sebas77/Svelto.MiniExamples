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
            return ref map.FindElement(id.entityID);
        }
    }
}