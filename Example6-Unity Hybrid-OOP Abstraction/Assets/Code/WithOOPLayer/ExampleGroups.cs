namespace Svelto.ECS.Example.OOPAbstraction.WithOOPLayer
{
    public static class ExampleGroups
    {
        public class Cube : GroupTag<Cube> { }
        public class Sphere : GroupTag<Sphere> { }

        //The specialised groups use the abstract Primitive tag so that the entities will be built in the 
        //expected groups
        public class CubePrimitive : GroupCompound<OOPLayer.LayerGroups.Primitive, Cube> { }
        public class SpherePrimitive : GroupCompound<OOPLayer.LayerGroups.Primitive, Sphere> { }
    }
}