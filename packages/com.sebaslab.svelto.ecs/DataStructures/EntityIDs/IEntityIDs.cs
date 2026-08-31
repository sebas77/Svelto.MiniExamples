namespace Svelto.ECS.Internal
{
    public interface IEntityIDs
    {
        uint this[int index] { get; }
        uint this[uint index] { get; }
    }
}