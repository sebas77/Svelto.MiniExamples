namespace Svelto.ECS.Example.OOPAbstraction.WithOOPLayer
{
    public static class ExampleGroups
    {
        public class Cube : GroupTag<Cube> { }
        public class Sphere : GroupTag<Sphere> { }

        public class CubePrimitive : GroupCompound<OOPLayer.LayerGroups.Primitive, Cube> { }
        public class SpherePrimitive : GroupCompound<OOPLayer.LayerGroups.Primitive, Sphere> { }
    }
}