using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityComponentBuilder : IEntityComponentBuilder
    {
        void Serialize(uint id, ITypeSafeDictionary dictionary, ISerializationData serializationData
                     , int serializationType);

        void Deserialize(uint id, ITypeSafeDictionary dictionary, ISerializationData serializationData
                       , int serializationType);

        void Deserialize(ISerializationData serializationData, in EntityComponentInitializer initializer
                       , int serializationType);
    }
}