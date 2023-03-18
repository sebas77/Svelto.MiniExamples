using System;
using System.Collections.Generic;
using DBC.ECS.Compute;
using Svelto.ECS.Internal;

namespace Svelto.ECS.ComputeSharp
{
    public class ComputeComponentBuilder<T> : IComponentBuilder where T : unmanaged, IEntityComputeSharpComponent
    {
        static readonly Type ENTITY_COMPONENT_TYPE;

        static ComputeComponentBuilder()
        {
            ENTITY_COMPONENT_TYPE = typeof(T);

            ComponentBuilderUtilities.CheckFields(ENTITY_COMPONENT_TYPE, false);
        }

        public ComputeComponentBuilder()
        {
            _initializer = default;
        }

        public ComputeComponentBuilder(in T initializer) : this()
        {
            _initializer = initializer;
        }

        public bool isUnmanaged => true;

        public void BuildEntityAndAddToList(ITypeSafeDictionary dictionary, EGID egid, IEnumerable<object> implementors)
        {
            var castedDic = dictionary as ITypeSafeDictionary<T>;

            Check.Require(!castedDic.ContainsKey(egid.entityID),
                $"building an entity with already used entity id! id: '{egid.entityID}'");

            castedDic.Add(egid.entityID, _initializer);
        }

        void IComponentBuilder.Preallocate(ITypeSafeDictionary dictionary, uint size)
        {
            Preallocate(dictionary, size);
        }

        public ITypeSafeDictionary CreateDictionary(uint size)
        {
            return ComputeSharpTypeSafeDictionary<T>.Create(size);
        }

        public Type GetEntityComponentType()
        {
            return ENTITY_COMPONENT_TYPE;
        }

        public override int GetHashCode()
        {
            return _initializer.GetHashCode();
        }

        static void Preallocate(ITypeSafeDictionary dictionary, uint size)
        {
            dictionary.EnsureCapacity(size);
        }

        readonly T _initializer;
    }
}