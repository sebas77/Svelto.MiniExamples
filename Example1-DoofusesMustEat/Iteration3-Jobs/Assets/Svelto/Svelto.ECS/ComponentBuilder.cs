using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;
using Svelto.Utilities;

namespace Svelto.ECS
{
    public class ComponentBuilder<T> : IEntityComponentBuilder where T : struct, IEntityComponent
    {
        public ComponentBuilder()
        {
            _initializer = DEFAULT_IT;

            EntityBuilderUtilities.CheckFields(ENTITY_COMPONENT_TYPE, NEEDS_REFLECTION);

            if (NEEDS_REFLECTION)
                EntityComponent.InitCache();
        }

        public ComponentBuilder(in T initializer) : this()
        {
            _initializer = initializer;
        }

        public void BuildEntityAndAddToList(ref ITypeSafeDictionary dictionary, EGID egid,
            IEnumerable<object> implementors)
        {
            if (dictionary == null) 
                dictionary = TypeSafeDictionaryFactory<T>.Create();

            var castedDic = dictionary as ITypeSafeDictionary<T>;

            if (NEEDS_REFLECTION)
            {
                DBC.ECS.Check.Require(implementors != null,
                    $"Implementors not found while building an EntityComponent `{typeof(T)}`");
                DBC.ECS.Check.Require(castedDic.ContainsKey(egid.entityID) == false,
                    $"building an entity with already used entity id! id: '{(ulong) egid}', {ENTITY_COMPONENT_NAME}");

                EntityComponent.BuildEntityComponent(out var entityComponent);

                this.FillEntityComponent(ref entityComponent, entityComponentBlazingFastReflection, implementors,
                    EntityComponent.implementorsByType, EntityComponent.cachedTypes);

                castedDic.Add(egid.entityID, entityComponent);
            }
            else
            {
                DBC.ECS.Check.Require(!castedDic.ContainsKey(egid.entityID),
                    $"building an entity with already used entity id! id: '{egid.entityID}'");

                castedDic.Add(egid.entityID, _initializer);
            }
        }

        ITypeSafeDictionary IEntityComponentBuilder.Preallocate(ref ITypeSafeDictionary dictionary, uint size)
        {
            return Preallocate(ref dictionary, size);
        }

        static ITypeSafeDictionary Preallocate(ref ITypeSafeDictionary dictionary, uint size)
        {
            if (dictionary == null)
                dictionary = TypeSafeDictionaryFactory<T>.Create(size);
            else
                dictionary.SetCapacity(size);

            return dictionary;
        }

        public Type GetEntityComponentType()
        {
            return ENTITY_COMPONENT_TYPE;
        }

        static ComponentBuilder()
        {
            ENTITY_COMPONENT_TYPE = typeof(T);
            DEFAULT_IT = default;
            NEEDS_REFLECTION = typeof(IEntityViewComponent).IsAssignableFrom(ENTITY_COMPONENT_TYPE);
            HAS_EGID = typeof(INeedEGID).IsAssignableFrom(ENTITY_COMPONENT_TYPE);
            ENTITY_COMPONENT_NAME = ENTITY_COMPONENT_TYPE.ToString();
#if UNITY_ECS            
            EntityComponentIDMap.Register<T>(new Filler<T>());
#endif            
            SetEGIDWithoutBoxing<T>.Warmup();
        }

        readonly T                        _initializer;

        static FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>> entityComponentBlazingFastReflection =>
            EntityComponent.cachedFields;

        internal static readonly Type ENTITY_COMPONENT_TYPE;
        public static readonly bool HAS_EGID;

        static readonly T      DEFAULT_IT;
        static readonly bool   NEEDS_REFLECTION;
        static readonly string ENTITY_COMPONENT_NAME;
        
        static class EntityComponent
        {
            internal static readonly FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>> cachedFields;
            internal static readonly Dictionary<Type, Type[]>                                cachedTypes;
#if DEBUG && !PROFILE_SVELTO
            internal static readonly Dictionary<Type, ECSTuple<object, int>> implementorsByType;
#else
            internal static readonly Dictionary<Type, object> implementorsByType;
#endif
            static EntityComponent()
            {
                cachedFields = new FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>>();

                var type = typeof(T);
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                for (var i = fields.Length - 1; i >= 0; --i)
                {
                    var field  = fields[i];
                    var setter = FastInvoke<T>.MakeSetter(field);

                    cachedFields.Add(new KeyValuePair<Type, FastInvokeActionCast<T>>(field.FieldType, setter));
                }

                cachedTypes = new Dictionary<Type, Type[]>();

#if DEBUG && !PROFILE_SVELTO
                implementorsByType = new Dictionary<Type, ECSTuple<object, int>>();
#else
                implementorsByType = new Dictionary<Type, object>();
#endif
            }

            internal static void InitCache()
            {}

            internal static void BuildEntityComponent(out T entityComponent)
            {
                entityComponent = new T();
            }
        }
    }
}