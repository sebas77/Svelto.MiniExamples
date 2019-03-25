#if UNITY_BURST_FEATURE_FUNCPTR

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace BurstHelper
{
    public static class MethodCompiler
    {
        /// <summary>
        ///     don't ask me why it's so complicated, I am hacking here. Use at your risk.
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ConvertBurstMethodToDelegate<T>(T action) where T : MulticastDelegate
        {
            var delegateType = DelegateHelper.NewDelegateType(action.Method.ReturnType, typeof(T).GetGenericArguments());
            var functionDelegate = Delegate.CreateDelegate(delegateType, action.Method);
            var o = BurstCompiler.CompileDelegate(functionDelegate);
            var compile = DelegateCompiler.Compile<T>(o.Method);

            return compile;
        }
    }

    /// <summary>
    ///     Copy and pasted from https://github.com/idavis/ThereBeDragons/tree/master/src/MethodEmitter
    ///     thank you Ian!
    /// </summary>
    static class DelegateCompiler
    {
        public static T Compile<T>(MethodInfo method)
        {
            if (typeof(Delegate).IsAssignableFrom(typeof(T)))
                return (T) (object) _Compile(method, typeof(T));
            throw new InvalidOperationException("Only delegate types are supported");
        }

        static Delegate _Compile(MethodInfo method, Type type)
        {
            var target = Delegate.CreateDelegate(type, method);
            return target;
        }
    }

    /// <summary>
    ///     Copy and pasted from Burst code
    /// </summary>
    static class DelegateHelper
    {
        static readonly Type[] _DelegateCtorSignature = new Type[2]
        {
            typeof(object),
            typeof(IntPtr)
        };

        static readonly Dictionary<DelegateKey, Type> DelegateTypes = new Dictionary<DelegateKey, Type>();

        public static Type NewDelegateType(Type ret, Type[] parameters)
        {
            var key = new DelegateKey(ret, (Type[]) parameters.Clone());
            Type delegateType;
            if (!DelegateTypes.TryGetValue(key, out delegateType))
            {
                var assemblyName = Guid.NewGuid().ToString();

                var name = new AssemblyName(assemblyName);
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name);
                assemblyBuilder.DefineVersionInfoResource();

                var typeBuilder = moduleBuilder.DefineType("CustomDelegate",
                                                           TypeAttributes.Public | TypeAttributes.Sealed |
                                                           TypeAttributes.AutoClass, typeof(MulticastDelegate));
                var constructor = typeof(UnmanagedFunctionPointerAttribute).GetConstructors()[0];

                // Make sure that we setup the C calling convention on the unmanaged delegate
                var customAttribute = new CustomAttributeBuilder(constructor, new object[] {CallingConvention.Cdecl});
                typeBuilder.SetCustomAttribute(customAttribute);
                typeBuilder
                   .DefineConstructor(
                        MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName,
                        CallingConventions.Standard, _DelegateCtorSignature)
                   .SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
                typeBuilder
                   .DefineMethod(
                        "Invoke",
                        MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig |
                        MethodAttributes.VtableLayoutMask, ret, parameters)
                   .SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
                delegateType = typeBuilder.CreateType();

                DelegateTypes.Add(key, delegateType);
            }

            return delegateType;
        }

        struct DelegateKey : IEquatable<DelegateKey>
        {
            public DelegateKey(Type returnType, Type[] arguments)
            {
                ReturnType = returnType;
                Arguments = arguments;
            }

            public readonly Type ReturnType;

            public readonly Type[] Arguments;

            public bool Equals(DelegateKey other)
            {
                if (ReturnType.Equals(other.ReturnType) && Arguments.Length == other.Arguments.Length)
                {
                    for (var i = 0; i < Arguments.Length; i++)
                        if (Arguments[i] != other.Arguments[i])
                            return false;
                    return true;
                }

                return false;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is DelegateKey && Equals((DelegateKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashcode = (ReturnType.GetHashCode() * 397) ^ Arguments.Length.GetHashCode();
                    for (var i = 0; i < Arguments.Length; i++) hashcode = (hashcode * 397) ^ Arguments[i].GetHashCode();
                    return hashcode;
                }
            }
        }
    }
}

#endif