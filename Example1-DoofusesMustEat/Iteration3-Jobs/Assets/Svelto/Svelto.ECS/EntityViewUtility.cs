using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS
{
#if DEBUG && !PROFILE_SVELTO
    struct ECSTuple<T1, T2>
    {
        public readonly T1 implementorType;
        public          T2 numberOfImplementations;

        public ECSTuple(T1 implementor, T2 v)
        {
            implementorType         = implementor;
            numberOfImplementations = v;
        }
    }
#endif

    static class EntityComponentUtility
    {

        public static void FillEntityComponent<T>(this IComponentBuilder componentBuilder, ref T entityComponent
                                           , FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>>
                                                 entityComponentBlazingFastReflection, IEnumerable<object> implementors,
#if DEBUG && !PROFILE_SVELTO
                                             Dictionary<Type, ECSTuple<object, int>> implementorsByType
#else
                                            Dictionary<Type, object> implementorsByType
#endif
                                           , Dictionary<Type, Type[]> cachedTypes
        )
        {
            //efficient way to collect the fields of every EntityComponentType
            var setters =
                FasterList<KeyValuePair<Type, FastInvokeActionCast<T>>>.NoVirt.ToArrayFast(entityComponentBlazingFastReflection, out var count);

            foreach (var implementor in implementors)
            {
                if (implementor != null)
                {
                    var type = implementor.GetType();

                    if (cachedTypes.TryGetValue(type, out var interfaces) == false)
                        interfaces = cachedTypes[type] = type.GetInterfacesEx();

                    for (var iindex = 0; iindex < interfaces.Length; iindex++)
                    {
                        var componentType = interfaces[iindex];
#if DEBUG && !PROFILE_SVELTO
                        if (implementorsByType.TryGetValue(componentType, out var implementorData))
                        {
                            implementorData.numberOfImplementations++;
                            implementorsByType[componentType] = implementorData;
                        }
                        else
                            implementorsByType[componentType] = new ECSTuple<object, int>(implementor, 1);
#else
                        implementorsByType[componentType] = implementor;
#endif
                    }
                }
#if DEBUG && !PROFILE_SVELTO
                else
                {
                    Console.Log(NULL_IMPLEMENTOR_ERROR.FastConcat(" entityComponent ",
                            componentBuilder.GetEntityComponentType().ToString()));
                }
#endif
            }

            for (var i = 0; i < count; i++)
            {
                var fieldSetter = setters[i];
                var fieldType   = fieldSetter.Key;

#if DEBUG && !PROFILE_SVELTO
                ECSTuple<object, int> component;
#else
                object component;
#endif

                if (implementorsByType.TryGetValue(fieldType, out component) == false)
                {
                    var e = new ECSException(NOT_FOUND_EXCEPTION + " Component Type: " + fieldType.Name +
                                             " - EntityComponent: "   + componentBuilder.GetEntityComponentType().Name);

                    throw e;
                }
#if DEBUG && !PROFILE_SVELTO
                if (component.numberOfImplementations > 1)
                    throw new ECSException(DUPLICATE_IMPLEMENTOR_ERROR.FastConcat(
                        "Component Type: ", fieldType.Name, " implementor: ", component.implementorType.ToString()) +
                                           " - EntityComponent: " + componentBuilder.GetEntityComponentType().Name);
#endif
#if DEBUG && !PROFILE_SVELTO
                fieldSetter.Value(ref entityComponent, component.implementorType);
#else
                fieldSetter.Value(ref entityComponent, component);
#endif
            }

            implementorsByType.Clear();
        }

        const string DUPLICATE_IMPLEMENTOR_ERROR =
            "<color=teal>Svelto.ECS</color> the same component is implemented with more than one implementor. This is " +
            "considered an error and MUST be fixed. ";

        const string NULL_IMPLEMENTOR_ERROR =
            "<color=teal>Svelto.ECS</color> Null implementor, please be careful about the implementors passed to avoid " +
            "performance loss ";

        const string NOT_FOUND_EXCEPTION = "<color=teal>Svelto.ECS</color> Implementor not found for an EntityComponent. ";
    }
}