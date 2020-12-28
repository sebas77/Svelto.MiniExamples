namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public static class ExampleGroups
    {
        public class Cube : GroupTag<Cube> { };
        public class Sphere : GroupTag<Sphere> { };
        public class Primitive : GroupTag<Primitive> { };

        public class CubePrimitive : GroupCompound<Primitive, Cube> { };
        public class SpherePrimitive : GroupCompound<Primitive, Sphere> { };
    }
}