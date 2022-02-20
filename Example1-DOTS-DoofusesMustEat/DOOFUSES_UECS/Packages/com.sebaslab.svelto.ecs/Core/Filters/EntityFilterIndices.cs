using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public readonly struct EntityFilterIndices
    {
        public EntityFilterIndices(NB<uint> indices, uint count)
        {
            _indices = indices;
            _count   = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count() => _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Get(uint index) => _indices[index];

        public uint this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _indices[index];
        }

        public uint this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _indices[index];
        }

        readonly NB<uint> _indices;
        readonly uint      _count;
    }
}