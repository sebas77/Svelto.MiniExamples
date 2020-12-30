namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    /// <summary>
    /// This is the entity descriptor the specialised layers should use to guarantee that this layer engines are
    /// going to work without troubles. This descriptor list the components needed to link an entity to an object
    /// </summary>
    public class
        PrimitiveEntityDescriptor : GenericEntityDescriptor<TransformComponent, ObjectIndexComponent, EGIDComponent> { }
}