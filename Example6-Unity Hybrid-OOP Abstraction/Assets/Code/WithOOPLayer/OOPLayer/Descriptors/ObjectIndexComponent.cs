using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public struct ObjectIndexComponent : IEntityComponent
    {
        public uint          index;
        public PrimitiveType type;

        public ObjectIndexComponent(PrimitiveType type) : this() { this.type = type; }
    }
}