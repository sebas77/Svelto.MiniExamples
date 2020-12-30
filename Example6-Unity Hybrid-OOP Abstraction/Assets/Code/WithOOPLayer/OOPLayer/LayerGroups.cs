namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    /// <summary>
    /// The abstract layer provides the tag the specialised entities must use in order to be processed by this layer
    /// engines
    /// </summary>
    public static class LayerGroups
    {
        public class Primitive : GroupTag<Primitive> { }
    }
}