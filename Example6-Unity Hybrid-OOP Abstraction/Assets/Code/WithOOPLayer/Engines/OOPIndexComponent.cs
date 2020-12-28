namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    struct OOPIndexComponent : IEntityComponent
    {
        public uint index;
        public uint parent;

        public OOPIndexComponent(uint index):this() { this.index = index; }
    }
}