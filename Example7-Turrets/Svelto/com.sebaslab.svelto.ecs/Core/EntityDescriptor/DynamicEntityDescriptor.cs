using System;
using System.Collections;
using System.Collections.Generic;
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
            var defaultComponents = EntityDescriptorTemplate<TType>.descriptor.componentsToBuild;
            var length            = defaultComponents.Length;

            _componentsToBuild = new IComponentBuilder[length + 1];

            Array.Copy(defaultComponents, 0, _componentsToBuild, 0, length);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            _componentsToBuild[length] = new ComponentBuilder<EntityInfoComponent>(new EntityInfoComponent
            {
                componentsToBuild = _componentsToBuild
            });
        }

        public DynamicEntityDescriptor(IComponentBuilder[] extraEntityBuilders) : this()
        {
            var extraComponentsLength = extraEntityBuilders.Length;

            _componentsToBuild = Construct(extraComponentsLength, extraEntityBuilders
                                         , EntityDescriptorTemplate<TType>.descriptor.componentsToBuild);
        }

        public DynamicEntityDescriptor(FasterList<IComponentBuilder> extraEntityBuilders) : this()
        {
            var extraComponents       = extraEntityBuilders.ToArrayFast(out _);
            var extraComponentsLength = extraEntityBuilders.count;

            _componentsToBuild = Construct((int) extraComponentsLength, extraComponents
                                         , EntityDescriptorTemplate<TType>.descriptor.componentsToBuild);
        }

        public void ExtendWith<T>() where T : IEntityDescriptor, new()
        {
            var newComponentsToBuild = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;

            _componentsToBuild = Construct(newComponentsToBuild.Length, newComponentsToBuild, _componentsToBuild);
        }

        public void ExtendWith(IComponentBuilder[] extraComponents)
        {
            _componentsToBuild = Construct(extraComponents.Length, extraComponents, _componentsToBuild);
        }

        public void ExtendWith(FasterList<IComponentBuilder> extraComponents)
        {
            _componentsToBuild =
                Construct(extraComponents.count, extraComponents.ToArrayFast(out _), _componentsToBuild);
        }

        public void Add<T>() where T : struct, IEntityComponent
        {
            _componentsToBuild = Construct(1, GenericEntityDescriptor<T>._componentBuilders, _componentsToBuild);
        }

        public void Add<T, U>() where T : struct, IEntityComponent where U : struct, IEntityComponent
        {
            _componentsToBuild = Construct(2, GenericEntityDescriptor<T, U>._componentBuilders, _componentsToBuild);
        }

        public void Add<T, U, V>()
            where T : struct, IEntityComponent where U : struct, IEntityComponent where V : struct, IEntityComponent
        {
            _componentsToBuild = Construct(3, GenericEntityDescriptor<T, U, V>._componentBuilders, _componentsToBuild);
        }

        static IComponentBuilder[] Construct
            (int extraComponentsLength, IComponentBuilder[] extraComponents, IComponentBuilder[] startingComponents)
        {
            IComponentBuilder[] localComponentsToBuild;

            if (extraComponentsLength == 0)
            {
                localComponentsToBuild = startingComponents;
                return localComponentsToBuild;
            }

            HashSet<IComponentBuilder> checkForDupes =
                new HashSet<IComponentBuilder>(extraComponents, new ComponentBuilderComparer());
            checkForDupes.UnionWith(startingComponents);
            checkForDupes.Remove(entityInfoComponent);
            localComponentsToBuild = new IComponentBuilder[checkForDupes.Count + 1];
            checkForDupes.CopyTo(localComponentsToBuild);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            localComponentsToBuild[localComponentsToBuild.Length - 1] = new ComponentBuilder<EntityInfoComponent>(
                new EntityInfoComponent
                {
                    componentsToBuild = localComponentsToBuild
                });

            return localComponentsToBuild;
        }

        static ComponentBuilder<EntityInfoComponent> entityInfoComponent = new ComponentBuilder<EntityInfoComponent>();

        public IComponentBuilder[] componentsToBuild => _componentsToBuild;

        IComponentBuilder[] _componentsToBuild;
    }
}