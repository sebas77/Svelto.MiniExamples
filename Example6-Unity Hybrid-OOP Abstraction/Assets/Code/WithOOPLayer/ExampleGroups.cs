namespace Svelto.ECS.Example.OOPAbstraction.WithOOPLayer
{
    public static class ExampleGroups
    {
        public class Cube : GroupTag<Cube> { }
        public class Sphere : GroupTag<Sphere> { }

        public class CubePrimitive : GroupCompound<OOPLayer.ExampleGroups.Primitive, Cube> { }
        public class SpherePrimitive : GroupCompound<OOPLayer.ExampleGroups.Primitive, Sphere> { }
    }
}