namespace Svelto.ECS
{
    internal struct SerializableEntityStruct : IEntityComponent, INeedEGID
    {
        public uint descriptorHash;

        public EGID ID { get; set; }
    }
}