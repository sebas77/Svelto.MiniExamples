namespace Svelto.ECS
{
    public interface IEntityDescriptor
    {
        IEntityBuilder[] entityComponentsToBuild { get; }
    }

    static class EntityDescriptorTemplate<TType> where TType : IEntityDescriptor, new()
    {
        static EntityDescriptorTemplate()
        {
            descriptor = new TType();
        }

        public static IEntityDescriptor descriptor { get; }
    }
}
