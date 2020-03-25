namespace Svelto.ECS
{
    public abstract class GenericEntityDescriptor<T> : IEntityDescriptor where T : struct,  IEntityComponent
    {
        static readonly IEntityBuilder[] _entityBuilders;
        static GenericEntityDescriptor() { _entityBuilders = new IEntityBuilder[] {new EntityBuilder<T>()}; }

        public IEntityBuilder[] entitiesToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U> : IEntityDescriptor
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent
    {
        static readonly IEntityBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityBuilder[] {new EntityBuilder<T>(), new EntityBuilder<U>()};
        }

        public IEntityBuilder[] entitiesToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V> : IEntityDescriptor
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent where V : struct,  IEntityComponent
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
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent where V : struct,  IEntityComponent
        where W : struct,  IEntityComponent
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
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent where V : struct,  IEntityComponent
        where W : struct,  IEntityComponent where X : struct,  IEntityComponent
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
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent where V : struct,  IEntityComponent
        where W : struct,  IEntityComponent where X : struct,  IEntityComponent where Y : struct,  IEntityComponent
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