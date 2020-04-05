namespace Svelto.ECS
{
    public abstract class GenericEntityDescriptor<T> : IEntityDescriptor where T : struct,  IEntityComponent
    {
        static readonly IEntityComponentBuilder[] _entityBuilders;
        static GenericEntityDescriptor() { _entityBuilders = new IEntityComponentBuilder[] {new ComponentBuilder<T>()}; }

        public IEntityComponentBuilder[] componentsToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U> : IEntityDescriptor
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent
    {
        static readonly IEntityComponentBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityComponentBuilder[] {new ComponentBuilder<T>(), new ComponentBuilder<U>()};
        }

        public IEntityComponentBuilder[] componentsToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V> : IEntityDescriptor
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent where V : struct,  IEntityComponent
    {
        static readonly IEntityComponentBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityComponentBuilder[]
            {
                new ComponentBuilder<T>(),
                new ComponentBuilder<U>(),
                new ComponentBuilder<V>()
            };
        }

        public IEntityComponentBuilder[] componentsToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W> : IEntityDescriptor
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent where V : struct,  IEntityComponent
        where W : struct,  IEntityComponent
    {
        static readonly IEntityComponentBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityComponentBuilder[]
            {
                new ComponentBuilder<T>(),
                new ComponentBuilder<U>(),
                new ComponentBuilder<V>(),
                new ComponentBuilder<W>()
            };
        }

        public IEntityComponentBuilder[] componentsToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W, X> : IEntityDescriptor
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent where V : struct,  IEntityComponent
        where W : struct,  IEntityComponent where X : struct,  IEntityComponent
    {
        static readonly IEntityComponentBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityComponentBuilder[]
            {
                new ComponentBuilder<T>(),
                new ComponentBuilder<U>(),
                new ComponentBuilder<V>(),
                new ComponentBuilder<W>(),
                new ComponentBuilder<X>()
            };
        }

        public IEntityComponentBuilder[] componentsToBuild => _entityBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W, X, Y> : IEntityDescriptor
        where T : struct,  IEntityComponent where U : struct,  IEntityComponent where V : struct,  IEntityComponent
        where W : struct,  IEntityComponent where X : struct,  IEntityComponent where Y : struct,  IEntityComponent
    {
        static readonly IEntityComponentBuilder[] _entityBuilders;

        static GenericEntityDescriptor()
        {
            _entityBuilders = new IEntityComponentBuilder[]
            {
                new ComponentBuilder<T>(),
                new ComponentBuilder<U>(),
                new ComponentBuilder<V>(),
                new ComponentBuilder<W>(),
                new ComponentBuilder<X>(),
                new ComponentBuilder<Y>()
            };
        }

        public IEntityComponentBuilder[] componentsToBuild => _entityBuilders;
    }
}