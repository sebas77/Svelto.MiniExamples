using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public struct GameObjectEntityComponent : IEntityComponent
    {
        public ValueIndex resourceIndex;
        public int layer;
    }
}
