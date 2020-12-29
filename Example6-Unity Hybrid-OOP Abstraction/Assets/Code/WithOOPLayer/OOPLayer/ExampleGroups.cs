namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public static class ExampleGroups
    {
        public class Primitive : GroupTag<Primitive> { }
        public class WithParent : GroupTag<WithParent> { }
        
        public class PrimitiveWithParent : GroupCompound<Primitive, WithParent> { }
    }
}