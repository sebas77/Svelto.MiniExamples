namespace Svelto.ECS
{
    public abstract class GenericEntityDescriptor<T> : IEntityDescriptor where T : IEntityStruct, new()
    {
        static readonly IEntityBuilder[] _entityBuilders;
        static GenericEntityDescriptor() { _entityBuilders = new IEntityBuilder[] {new EntityBuilder<T>()}; }

        public IEntityBuilder[] entitiesToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U> : IEntityDescriptor
        where T : IEntityStruct, new() where U : IEntityStruct, new()
    {
        static readonly IEntityBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityBuilder[] {new EntityBuilder<T>(), new EntityBuilder<U>()};
        }

        public IEntityBuilder[] entitiesToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V> : IEntityDescriptor
        where T : IEntityStruct, new() where U : IEntityStruct, new() where V : IEntityStruct, new()
    {
        static readonly IEntityBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityBuilder[]
            {
                new EntityBuilder<T>(),
                new EntityBuilder<U>(),
                new EntityBuilder<V>()
            };
        }

        public IEntityBuilder[] entitiesToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W> : IEntityDescriptor
        where T : IEntityStruct, new() where U : IEntityStruct, new() where V : IEntityStruct, new()
        where W : IEntityStruct, new()
    {
        static readonly IEntityBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityBuilder[]
            {
                new EntityBuilder<T>(),
                new EntityBuilder<U>(),
                new EntityBuilder<V>(),
                new EntityBuilder<W>()
            };
        }

        public IEntityBuilder[] entitiesToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W, X> : IEntityDescriptor
        where T : IEntityStruct, new() where U : IEntityStruct, new() where V : IEntityStruct, new()
        where W : IEntityStruct, new() where X : IEntityStruct, new()
    {
        static readonly IEntityBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityBuilder[]
            {
                new EntityBuilder<T>(),
                new EntityBuilder<U>(),
                new EntityBuilder<V>(),
                new EntityBuilder<W>(),
                new EntityBuilder<X>()
            };
        }

        public IEntityBuilder[] entitiesToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W, X, Y> : IEntityDescriptor
        where T : IEntityStruct, new() where U : IEntityStruct, new() where V : IEntityStruct, new()
        where W : IEntityStruct, new() where X : IEntityStruct, new() where Y : IEntityStruct, new()
    {
        static readonly IEntityBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityBuilder[]
            {
                new EntityBuilder<T>(),
                new EntityBuilder<U>(),
                new EntityBuilder<V>(),
                new EntityBuilder<W>(),
                new EntityBuilder<X>(),
                new EntityBuilder<Y>()
            };
        }

        public IEntityBuilder[] entitiesToBuild => _entityBuilders;
    }
}