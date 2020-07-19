using System;
using System.Runtime.CompilerServices;

namespace Svelto.DataStructures
{
    public interface IBufferBase<T>
    {
    }
    
    public interface IBuffer<T>:IBufferBase<T>
    {
        //ToDo to remove (only implementation can be used)
        ref T this[uint index] { get; }
        //ToDo to remove(only implementation can be used)
        ref T this[int index] { get; }
        
        void CopyTo(uint sourceStartIndex, T[] destination, uint destinationStartIndex, uint size);
        void Clear();
        
        T[]    ToManagedArray();
        IntPtr ToNativeArray(out int capacity);

        int capacity { get; }
    }
    
    public static class IBufferExtensionN
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NB<T> ToFast<T>(this IBuffer<T> buffer) where T:unmanaged
        {
            DBC.Common.Check.Assert(buffer is NB<T>, "impossible conversion");
            return (NB<T>) buffer;
        }
    }
    
    public static class IBufferExtensionM
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MB<T> ToFast<T>(this IBufferBase<T> buffer)
        {
            DBC.Common.Check.Assert(buffer is MB<T>, "impossible conversion");
            return (MB<T>) buffer;
        }
    }
}