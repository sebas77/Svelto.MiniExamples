#if NEW_C_SHARP || !UNITY_5_3_OR_NEWER
using System;
using Svelto.Common;

namespace Svelto.DataStructures
{
    /// <summary>
    /// ManagedVO, which are VOs with arrays, use a more complicated pattern as they cannot be blitted as they are
    /// the ISerializableManagedVO helps computing the actual size of the VO, including the arrays.
    /// more tricks are used to speed up serialization: the Sequential and Packed 1 layout helps to serialise
    /// the unmanaged part of the struct that must always stay at the beginning of the struct.
    /// _constSize is used to compute the size of the unmanaged part of the struct, which is always the same.
    /// the rest of the struct is serialised using the UnmanagedStream interface
    /// example:
///    [StructLayout(LayoutKind.Sequential, Pack = 1)] //important otherwise the serialization trick for managed VOs won't work
///    public struct ProjectileBouncingEffectVO : ISerializableManagedVO
///    {
///        public UniqueId id;
///        public HashId effect_hash;
///        public UniqueId caster_id;
///
///        public UniqueId source_id;
///        public float source_x;
///        public float source_z;
///
///        // Bouncing projectiles have a duration that is all the hit times added together
///        public float duration;
///
///        //in order to keep serialization code minimal, managed stuff must stay at the end of the declaration
///        public ByteArraySegment<float> hit_times;
///        public ByteArraySegment<long> hit_target_ids;
///   }
///
///   ByteArraySegment is also provided by Svelto.Common and must be used to serialise arrays (of unmanaged types)
///
/// </summary>
    public interface ISerializableManagedVO
    {
        int SerializationSize();
        void Serialize(ref UnmanagedStream stream);
    }

    public static class ManagerVOSerializationHelper
    {
        //this can be used like:
//        FasterList<ProjectileBouncingEffectVO> bouncingProjectileEffectsBuffer = _valueObjectSystem.GetBouncingProjectileEffects();
//                if (bouncingProjectileEffectsBuffer.count > 0)
//        {
//            using (platformProfiler.Sample("effects_proj_bounce serialization"))
//            {
//                unsafe
//                {
//                    int serializeSize = bouncingProjectileEffectsBuffer.SerializeSize();
//                    byte* span = stackalloc byte[serializeSize]; //stack allocation
//                    var serializedVOs = new UnmanagedStream(span, serializeSize);
//                    realmVO.effects_proj_bounce = (bouncingProjectileEffectsBuffer.Serialize(ref serializedVOs));
//                }
//            }
//        }
        public static Span<byte> Serialize<T>(this FasterList<T> elements,
            ref UnmanagedStream buffer) where T:ISerializableManagedVO
        {
            foreach (ref T element in elements)
            {
                element.Serialize(ref buffer);
            }

            return buffer.AsSpan();
        }

        public static int SerializeSize<T>(this FasterList<T> elements)  where T:ISerializableManagedVO
        {
            if (elements.count > 0)
            {
                int serializationSize = 0;
                foreach (ref T element in elements)
                {
                    serializationSize += element.SerializationSize(); //size can change if T has dynamic buffers inside
                }

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