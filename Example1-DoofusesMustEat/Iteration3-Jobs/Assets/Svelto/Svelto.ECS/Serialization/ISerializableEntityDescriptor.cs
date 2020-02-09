namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityDescriptor : IEntityDescriptor
    {
        uint                         hash                { get; }
        ISerializableEntityBuilder[] entitiesToSerialize { get; }
    }
}