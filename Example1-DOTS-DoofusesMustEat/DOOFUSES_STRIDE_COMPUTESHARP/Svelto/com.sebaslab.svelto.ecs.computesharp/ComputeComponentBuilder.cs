using System;
using System.Collections.Generic;
using DBC.ECS.Compute;
using Svelto.Common;
using Svelto.ECS.Internal;

namespace Svelto.ECS.ComputeSharp
{
    public class ComputeComponentBuilder<T> : IComponentBuilder where T : unmanaged, IEntityComponent
    {
        static readonly Type ENTITY_COMPONENT_TYPE;

        static readonly T      DEFAULT_IT;
        static readonly string ENTITY_COMPONENT_NAME;
        static readonly bool   IS_UNMANAGED;

        static ComputeComponentBuilder()
        {
            ENTITY_COMPONENT_TYPE = typeof(T);
            DEFAULT_IT = default;

            ComponentID<T>.Init();
            ENTITY_COMPONENT_NAME = ENTITY_COMPONENT_TYPE.ToString();
            IS_UNMANAGED = TypeType.isUnmanaged<T>(); //attention this is important as it serves as warm up for Type<T>

            ComponentBuilderUtilities.CheckFields(ENTITY_COMPONENT_TYPE, false);
        }

        public ComputeComponentBuilder()
        {
            _initializer = DEFAULT_IT;
        }

        public ComputeComponentBuilder(in T initializer) : this()
        {
            _initializer = initializer;
        }

        public bool isUnmanaged => IS_UNMANAGED;

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