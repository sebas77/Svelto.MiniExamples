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

            ComponentsToBuild = new IComponentBuilder[length + 1];

            Array.Copy(defaultEntities, 0, ComponentsToBuild, 0, length);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            ComponentsToBuild[length] = new ComponentBuilder<EntityInfoComponent>(new EntityInfoComponent
            {
                componentsToBuild = ComponentsToBuild
            });
        }

        public DynamicEntityDescriptor(IComponentBuilder[] extraEntityBuilders) : this()
        {
            var extraEntitiesLength = extraEntityBuilders.Length;

            ComponentsToBuild = Construct(extraEntitiesLength, extraEntityBuilders
                                        , EntityDescriptorTemplate<TType>.descriptor.componentsToBuild);
        }

        public DynamicEntityDescriptor(FasterList<IComponentBuilder> extraEntityBuilders) : this()
        {
            var extraEntities       = extraEntityBuilders.ToArrayFast(out _);
            var extraEntitiesLength = extraEntityBuilders.count;

            ComponentsToBuild = Construct((int)extraEntitiesLength, extraEntities
                                        , EntityDescriptorTemplate<TType>.descriptor.componentsToBuild);
        }

        public void ExtendWith<T>() where T : IEntityDescriptor, new()
        {
            var newEntitiesToBuild = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;

            ComponentsToBuild = Construct(newEntitiesToBuild.Length, newEntitiesToBuild, ComponentsToBuild);
        }

        public void ExtendWith(IComponentBuilder[] extraEntities)
        {
            ComponentsToBuild = Construct(extraEntities.Length, extraEntities, ComponentsToBuild);
        }

        public void ExtendWith(FasterList<IComponentBuilder> extraEntities)
        {
            ComponentsToBuild = Construct(extraEntities.count, extraEntities.ToArrayFast(out _), ComponentsToBuild);
        }

        static IComponentBuilder[] Construct
            (int extraComponentsLength, IComponentBuilder[] extraComponents, IComponentBuilder[] startingComponents)
        {
            int RemoveDuplicates()
            {
                var components = new FasterDictionary<RefWrapper<IComponentBuilder, ComponentBuilderComparer>,
                            IComponentBuilder>();
                var xtraComponents = new FasterDictionary<RefWrapper<IComponentBuilder, ComponentBuilderComparer>,
                            IComponentBuilder>();

                for (uint i = 0; i < startingComponents.Length; i++)
                    components[new RefWrapper<IComponentBuilder, ComponentBuilderComparer>(startingComponents[i])] =
                        startingComponents[i];

                for (uint i = 0; i < extraComponentsLength; i++)
                    xtraComponents[new RefWrapper<IComponentBuilder, ComponentBuilderComparer>(extraComponents[i])] =
                        extraComponents[i];

                xtraComponents.Exclude(components);

                if (extraComponentsLength != xtraComponents.count)
                {
                    extraComponentsLength = xtraComponents.count;

                    uint index = 0;
                    foreach (var couple in xtraComponents)
                        extraComponents[index++] = couple.Key.value;
                }

                return extraComponentsLength;
            }

            IComponentBuilder[] localEntitiesToBuild;

            if (extraComponentsLength == 0)
            {
                localEntitiesToBuild = startingComponents;
                return localEntitiesToBuild;
            }

            extraComponentsLength = RemoveDuplicates();

            var entityInfoComponentIndex =
                SetupEntityInfoComponent(startingComponents, out localEntitiesToBuild, extraComponentsLength);

            Array.Copy(extraComponents, 0, localEntitiesToBuild, startingComponents.Length, extraComponentsLength);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            localEntitiesToBuild[entityInfoComponentIndex] = new ComponentBuilder<EntityInfoComponent>(
                new EntityInfoComponent
                {
                    componentsToBuild = localEntitiesToBuild
                });

            return localEntitiesToBuild;
        }

        public void Add<T>() where T : struct, IEntityComponent
        {
            IComponentBuilder[] extraEntities = { new ComponentBuilder<T>() };
            ComponentsToBuild = Construct(extraEntities.Length, extraEntities, ComponentsToBuild);
        }

        public void Add<T, U>() where T : struct, IEntityComponent where U : struct, IEntityComponent
        {
            IComponentBuilder[] extraEntities = { new ComponentBuilder<T>(), new ComponentBuilder<U>() };
            ComponentsToBuild = Construct(extraEntities.Length, extraEntities, ComponentsToBuild);
        }

        public void Add<T, U, V>() where T : struct, IEntityComponent
                                   where U : struct, IEntityComponent
                                   where V : struct, IEntityComponent
        {
            IComponentBuilder[] extraEntities =
            {
                new ComponentBuilder<T>(), new ComponentBuilder<U>(), new ComponentBuilder<V>()
            };
            ComponentsToBuild = Construct(extraEntities.Length, extraEntities, ComponentsToBuild);
        }

        static int SetupEntityInfoComponent
            (IComponentBuilder[] defaultEntities, out IComponentBuilder[] componentsToBuild, int extraLenght)
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

            if (index == -1)
            {
                index             = length + extraLenght;
                componentsToBuild = new IComponentBuilder[index + 1];
            }
            else
                componentsToBuild = new IComponentBuilder[length + extraLenght];

            Array.Copy(defaultEntities, 0, componentsToBuild, 0, length);

            return index;
        }

        public IComponentBuilder[] componentsToBuild => ComponentsToBuild;

        IComponentBuilder[] ComponentsToBuild;
    }
}