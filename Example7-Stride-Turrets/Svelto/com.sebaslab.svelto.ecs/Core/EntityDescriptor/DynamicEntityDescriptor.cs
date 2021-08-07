using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// DynamicEntityDescriptor can be used to add entity components to an existing EntityDescriptor that act as flags,
    /// at building time.
    /// This method allocates, so it shouldn't be abused
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public struct DynamicEntityDescriptor<TType> : IDynamicEntityDescriptor where TType : IEntityDescriptor, new()
    {
        internal DynamicEntityDescriptor(bool isExtendible) : this()
        {
            var defaultEntities = EntityDescriptorTemplate<TType>.descriptor.componentsToBuild;
            var length          = defaultEntities.Length;

            _componentsToBuild = new IComponentBuilder[length + 1];

            Array.Copy(defaultEntities, 0, _componentsToBuild, 0, length);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            _componentsToBuild[length] = new ComponentBuilder<EntityInfoComponent>(new EntityInfoComponent
            {
                componentsToBuild = _componentsToBuild
            });
        }

        public DynamicEntityDescriptor(IComponentBuilder[] extraEntityBuilders) : this(true)
        {
            var extraEntitiesLength = extraEntityBuilders.Length;

            _componentsToBuild = Construct(extraEntitiesLength, extraEntityBuilders);
        }

        public DynamicEntityDescriptor(FasterList<IComponentBuilder> extraEntityBuilders) : this(true)
        {
            var extraEntities       = extraEntityBuilders.ToArrayFast(out _);
            var extraEntitiesLength = extraEntityBuilders.count;

            _componentsToBuild = Construct((int)extraEntitiesLength, extraEntities);
        }

        public void ExtendWith<T>() where T : IEntityDescriptor, new()
        {
            var extraEntities = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;

            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }

        public void ExtendWith(IComponentBuilder[] extraEntities)
        {
            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }

        public void ExtendWith(FasterList<IComponentBuilder> extraEntities)
        {
            _componentsToBuild = Construct(extraEntities.count, extraEntities.ToArrayFast(out _));
        }

        public void Add<T>() where T : struct, IEntityComponent
        {
            IComponentBuilder[] extraEntities = { new ComponentBuilder<T>() };
            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }

        public void Add<T, U>() where T : struct, IEntityComponent where U : struct, IEntityComponent
        {
            IComponentBuilder[] extraEntities = { new ComponentBuilder<T>(), new ComponentBuilder<U>() };
            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }

        public void Add<T, U, V>() where T : struct, IEntityComponent
                                   where U : struct, IEntityComponent
                                   where V : struct, IEntityComponent
        {
            IComponentBuilder[] extraEntities =
            {
                new ComponentBuilder<T>(), new ComponentBuilder<U>(), new ComponentBuilder<V>()
            };
            _componentsToBuild = Construct(extraEntities.Length, extraEntities);
        }
        
        IComponentBuilder[] Construct(int extraComponentsLength, IComponentBuilder[] extraComponents)
        {
            static IComponentBuilder[] MergeLists
                (IComponentBuilder[] extraComponents, IComponentBuilder[] startingComponents, int extraComponentsLength)
            {
                var startComponents =
                    new FasterDictionary<RefWrapper<IComponentBuilder, ComponentBuilderComparer>, IComponentBuilder>();
                var mergedComponents =
                    new FasterDictionary<RefWrapper<IComponentBuilder, ComponentBuilderComparer>, IComponentBuilder>();

                for (uint i = 0; i < startingComponents.Length; i++)
                    startComponents
                            [new RefWrapper<IComponentBuilder, ComponentBuilderComparer>(startingComponents[i])] =
                        startingComponents[i];

                for (uint i = 0; i < extraComponentsLength; i++)
                    mergedComponents[new RefWrapper<IComponentBuilder, ComponentBuilderComparer>(extraComponents[i])] =
                        extraComponents[i];

                mergedComponents.Union(startComponents);

                IComponentBuilder[] componentBuilders = new IComponentBuilder[mergedComponents.count];
                
                mergedComponents.CopyValuesTo(componentBuilders);
                
                var entityInfoComponentIndex = FetchEntityInfoComponent(componentBuilders);

                componentBuilders[entityInfoComponentIndex] = new ComponentBuilder<EntityInfoComponent>(new EntityInfoComponent
                {
                    componentsToBuild = componentBuilders
                });

                return componentBuilders;
            }

            static int FetchEntityInfoComponent(IComponentBuilder[] defaultEntities)
            {
                int length = defaultEntities.Length;
                int index  = -1;

                for (var i = 0; i < length; i++)
                {
                    //the special entity already exists
                    if (defaultEntities[i].GetEntityComponentType() == ComponentBuilderUtilities.ENTITY_INFO_COMPONENT)
                    {
                        index = i;
                        break;
                    }
                }
                
                DBC.ECS.Check.Ensure(index != -1);

                return index;
            }

            if (extraComponentsLength == 0)
            {
                return _componentsToBuild;
            }

            return MergeLists(extraComponents, _componentsToBuild, extraComponentsLength);
        }

        public IComponentBuilder[] componentsToBuild => _componentsToBuild;

        IComponentBuilder[] _componentsToBuild;
    }
}