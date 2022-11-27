using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Example.Survive.Player
{
    public struct PlayerEntityComponent : IEntityComponent
    {
        public ValueIndex resourceIndex;
    }
}
