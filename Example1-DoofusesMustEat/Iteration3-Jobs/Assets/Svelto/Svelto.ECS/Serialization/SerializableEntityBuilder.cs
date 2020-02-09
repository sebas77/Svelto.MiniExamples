using System;
using Svelto.Common;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public class SerializableEntityBuilder<T> : EntityBuilder<T>, ISerializableEntityBuilder
        where T : unmanaged, IEntityStruct
    {
        public static readonly uint SIZE = UnsafeUtils.SizeOf<T>();

        static SerializableEntityBuilder()
        {}

        public SerializableEntityBuilder(params ValueTuple<SerializationType, ISerializer<T>>[] serializers)
        {
            _serializers = new ISerializer<T>[(int) SerializationType.Length];
            for (int i = 0; i < serializers.Length; i++)
            {
                ref (SerializationType, ISerializer<T>) s = ref serializers[i];
                _serializers[(int) s.Item1] = s.Item2;
            }

            // Just in case the above are the same type
            for (int i = 0; i < (int) SerializationType.Length; i++)
            {
                if (_serializers[i] == null) _serializers[i] = new DontSerialize<T>();
            }
        }

        public void Serialize(uint entityID, ITypeSafeDictionary dictionary,
            ISerializationData serializationData, SerializationType serializationType)
        {
            ISerializer<T> serializer = _serializers[(int)serializationType];

            var safeDictionary = (ITypeSafeDictionary<T>) dictionary;
            if (safeDictionary.TryFindIndex(entityID, out uint index) == false)
            {
                throw new ECSException("Entity Serialization failed");
            }

            var values = safeDictionary.unsafeValues;
            ref T val = ref values[(int) index];

            serializationData.dataPos = (uint) serializationData.data.count;

            serializationData.data.ExpandBy(serializer.size);
            serializer.SerializeSafe(val, serializationData);
        }

        public void Deserialize(uint entityID, ITypeSafeDictionary dictionary,
            ISerializationData serializationData, SerializationType serializationType)
        {
            ISerializer<T> serializer = _serializers[(int) serializationType];

            // Handle the case when an entity struct is gone
            var safeDictionary = (ITypeSafeDictionary<T>) dictionary;
            if (safeDictionary.TryFindIndex(entityID, out uint index) == false)
            {
                throw new ECSException("Entity Deserialization failed");
            }

            var values = safeDictionary.unsafeValues;
            ref T val = ref values[(int) index];

            serializer.DeserializeSafe(ref val, serializationData);
        }

        public void Deserialize(ISerializationData serializationData
            , in EntityStructInitializer initializer, SerializationType serializationType)
        {
            ISerializer<T> serializer = _serializers[(int) serializationType];

            serializer.DeserializeSafe(ref initializer.GetOrCreate<T>(), serializationData);
        }

        public void Deserialize(ISerializationData serializationData, ref T entityStruct, SerializationType serializationType)
        {
            ISerializer<T> serializer = _serializers[(int) serializationType];
            serializer.DeserializeSafe(ref entityStruct, serializationData);
        }

        readonly ISerializer<T>[] _serializers;
    }
}