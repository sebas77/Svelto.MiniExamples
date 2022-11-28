#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.DataStructures
{
    public interface ISerializableManagedVO
    {
        int SerializationSize();
        void Serialize(ref UnmanagedStream stream);
    }
    
    public static class ManagerVOSerializationHelper
    {
        public static Span<byte> Serialize<T>(this FasterList<T> elements,
            ref UnmanagedStream buffer) where T:ISerializableManagedVO
        {
            foreach (ref T element in elements)
            {
                element.Serialize(ref buffer);
            }

            return buffer.ToSpan();
        }

        public static int SerializeSize<T>(this FasterList<T> elements)  where T:ISerializableManagedVO
        {
            if (elements.count > 0)
            {
                var serializationSize = elements.count * elements[0].SerializationSize();
                return serializationSize;
            }

            return 0;
        }
        
        public static int SerializeSize<T>(this in ByteArraySegment<T> elements)  where T:unmanaged
        {
            var length = elements.Span.Length;
            
            if (length > 0)
            {
                var elementsLength = (length * MemoryUtilities.SizeOf<T>()) + sizeof(int);
                return elementsLength;
            }

            return sizeof(int);
        }
    }
}
#endif