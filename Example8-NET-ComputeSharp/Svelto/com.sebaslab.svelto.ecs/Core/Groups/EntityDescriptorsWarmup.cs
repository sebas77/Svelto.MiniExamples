using System;
using System.Collections.Generic;
using System.Reflection;

namespace Svelto.ECS
{
    public static class EntityDescriptorsWarmup
    {
        /// <summary>
        /// c# Static constructors are guaranteed to be thread safe
        /// Warmup all EntityDescriptors and ComponentTypeID classes to avoid huge overheads when they are first used
        /// </summary>
        internal static void Init()
        {
            List<Assembly> assemblies = AssemblyUtility.GetCompatibleAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    var typeOfEntityDescriptors = typeof(IEntityDescriptor);

                    foreach (Type type in AssemblyUtility.GetTypesSafe(assembly))
                    {
                        if (typeOfEntityDescriptors.IsAssignableFrom(type)) //IsClass and IsSealed and IsAbstract means only static classes
                        {
                            var warmup = typeof(EntityDescriptorTemplate<>).MakeGenericType(type);
                            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(warmup.TypeHandle);
                            PropertyInfo field = warmup.GetProperty("descriptor", BindingFlags.Static | BindingFlags.Public);
                            object value = field.GetValue(null); // pass null because the field is static

// cast the value to your descriptor type
                            IEntityDescriptor descriptor = (IEntityDescriptor)value;
                            foreach (IComponentBuilder component in descriptor.componentsToBuild)
                            {
                                var typeArguments = component.GetEntityComponentType();
                                var warmup2 = typeof(ComponentTypeID<>).MakeGenericType(typeArguments);
                                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(warmup2.TypeHandle);
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}