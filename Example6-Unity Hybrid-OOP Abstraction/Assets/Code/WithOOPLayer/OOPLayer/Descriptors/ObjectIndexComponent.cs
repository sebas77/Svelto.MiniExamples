namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public readonly struct ObjectIndexComponent : IEntityComponent
    {
        public readonly uint index;

        public ObjectIndexComponent(uint index) : this() { this.index = index; }
    }
}