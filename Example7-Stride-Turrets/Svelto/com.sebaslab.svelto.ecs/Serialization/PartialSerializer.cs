using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Svelto.DataStructures;
using Attribute = System.Attribute;

namespace Svelto.ECS.Serialization
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PartialSerializerFieldAttribute : Attribute
    {}

    //!!! DONT USE THIS SERIALIZER FOR NOW.
    /*
    Marshal.SizeOf(bool) will always return 4 for booleans, even if they don't occupy that size in memory!
    
    This is because Marshal.Sizeof has no idea about the whole struct we are serializing.
    Bools could be 1, 2 or 4 bytes depending on alignment and padding etc.
    Example:
    
    struct S {
        int x = 42;
        bool a = true;
        bool b = true;
    }
    
    Before we read booleans as 4 bytes due to our use of Marshal.Sizeof,
    such a struct with the given values was serialized like this:
    
    2A 00 00 00 <-- 42 little-endian
    01 01 00 00 <-- Both booleans as 1 byte
    00 00 00 00 <-- what the fuck
    
    We ended up also with extra padding at the end because the serializer uses the sum of
    each Marshal.Sizeof, so sizeof(int) + 2*sizeof(bool) is 12 bytes because `bool` is always 4.
    However, because serialization is done using `Marshal.OffsetOf<T>`, we still end up with
    the same data as in memory. And it turns out, IN MEMORY, these bools take 1 byte each.
    When we write the first bool, it writes 4 bytes: `01 01 00 00`. Both bools in here!
    When we write the second bool, it writes `01 00 00 00` one byte later.
    The trailing zeroes come from padding, but here we can also see with `b` that we are reading
    one more byte past the end of the struct!
    
    To workaround this problem from old saves, now if we use explicit struct layout.
    It kinda workarounds the problem of bounds.
    
    [StructLayout(LayoutKind.Explicit)]
    struct S {
        [FieldOffset(0)] int x = 42;
        [FieldOffset(4)] bool a = true;
        [FieldOffset(8)] bool b = true;
    }
    
    Will get serialized as:
    
    2A 00 00 00
    01 00 00 00
    01 00 00 00
    
    Booleans will both be forced to take 4 bytes in memory to match the serializer's behavior.
    The first one because the next one is 4 bytes after,
    and the second one because the struct has to be aligned to 4 bytes.
    However this is still not a good solution:
    - The serialized size doesn't change but the significant data will end up at different
      locations, so old saves that weren't using explicit layout can still have wrong data.
    - If the struct didn't start with an `int x` alignment rules could have been different.
    - We are still writing more bytes than necessary.
    
    So in short:
    STOP USING THIS SERIALIZER, especially if your struct contains booleans.
    If we want a similar functionality, we have to create a new serializer behaving more
    predictably.
    */
    public class PartialSerializer<T> : IComponentSerializer<T>
        where T : unmanaged, IEntityComponent
    {
        static PartialSerializer()
        {
            Type myType = typeof(T);
            FieldInfo[] myMembers = myType.GetFields();

            for (int i = 0; i < myMembers.Length; i++)
            {
                Object[] myAttributes = myMembers[i].GetCustomAttributes(true);
                for (int j = 0; j < myAttributes.Length; j++)
                {
                    if (myAttributes[j] is PartialSerializerFieldAttribute)
                    {
                        var fieldType = myMembers[i].FieldType;
                        if (fieldType.ContainsCustomAttribute(typeof(DoNotSerializeAttribute)) &&
                            myMembers[i].IsPrivate == false)
                                throw new ECSException($"field cannot be serialised {fieldType} in {myType.FullName}");

                        var offset = Marshal.OffsetOf<T>(myMembers[i].Name);
                        var sizeOf = (uint)Marshal.SizeOf(fieldType); // !!! See big comment on top of the class
                        offsets.Add(((uint) offset.ToInt32(), sizeOf));
                        totalSize += sizeOf; // !!! See big comment on top of the class
                    }
                }
            }

            if (myType.IsExplicitLayout == false)
                throw new ECSException($"PartialSerializer requires explicit layout {myType}");
#if SLOW_SVELTO_SUBMISSION            
            if (myType.GetProperties().Length > (ComponentBuilder<T>.HAS_EGID ? 1 : 0))
                throw new ECSException("serializable entity struct must be property less ".FastConcat(myType.FullName));
#endif                
        }

        public bool Serialize(in T value, ISerializationData serializationData) 
        {
            unsafe
            {
                fixed (byte* dataptr = serializationData.data.ToArrayFast(out _))
                {
                    var entityComponent = value;
                    foreach ((uint offset, uint size) offset in offsets)
                    {
                        byte* srcPtr = (byte*) &entityComponent + offset.offset;
                        //todo move to Unsafe Copy when available as it is faster
                        Buffer.MemoryCopy(srcPtr, dataptr + serializationData.dataPos,
                            serializationData.data.count - serializationData.dataPos, offset.size);
                        serializationData.dataPos += offset.size;
                    }
                }
            }

            return true;
        }

        public bool Deserialize(ref T value, ISerializationData serializationData) 
        {
            unsafe
            {
                T tempValue = value; //todo: temporary solution I want to get rid of this copy
                fixed (byte* dataptr = serializationData.data.ToArrayFast(out _))
                    foreach ((uint offset, uint size) offset in offsets)
                    {
                        byte* dstPtr = (byte*) &tempValue + offset.offset;
                        //todo move to Unsafe Copy when available as it is faster
                        Buffer.MemoryCopy(dataptr + serializationData.dataPos, dstPtr, offset.size, offset.size);
                        serializationData.dataPos += offset.size;
                    }

                value = tempValue; //todo: temporary solution I want to get rid of this copy
            }

            return true;
        }

        public uint size => totalSize;

        static readonly FasterList<(uint, uint)> offsets = new FasterList<(uint, uint)>();
        static readonly uint totalSize;
    }
}