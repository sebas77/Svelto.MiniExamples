using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MethodEmitter
{
    public static class CilMethodGenerator
    {
        public static T Compile<T>(MethodInfo method)
        {
            if(typeof(Delegate).IsAssignableFrom(typeof(T)))
                return (T)(object)_Compile(method);
            throw new InvalidOperationException("Only delegate types are supported");
        }

        private static Delegate _Compile(MethodInfo method)
        {
            var parameters = _GetParameters(method);
            var isMethodVoid = _IsMethodVoid(method);

            var dynamicMethod = new DynamicMethod(_GetAnonymousMethodName(), isMethodVoid ? null : method.ReturnType, parameters, method.DeclaringType, true);
          
            ILGenerator il = dynamicMethod.GetILGenerator();
            _EmitArguments(il, parameters.Length);
    
            il.Emit(OpCodes.Call, method);
            il.Emit(OpCodes.Ret);

            var delegateType = isMethodVoid
                ? _GetActionDelegateType(parameters)
                : _GetFuncDelegateType(method, parameters);

            var action = dynamicMethod.CreateDelegate(delegateType);
            
            return action;
        }

        private static Type[] _GetParameters(MethodInfo method)
        {
            var parameters = method.GetParameters().Select(prop => prop.ParameterType).ToArray();
            if (!method.IsStatic)
            {
                parameters = new[] {method.DeclaringType}.Concat(parameters).ToArray();
            }
            return parameters;
        }

        private static Type _GetFuncDelegateType(MethodInfo method, Type[] parameters)
        {
            Type delegateType;
            var args = parameters.Concat(new[] {method.ReturnType}).ToArray();
            Type openBaseType;
            if (DelegateMap.FuncsByParameterNumber.TryGetValue(args.Length, out openBaseType))
            {
                delegateType = openBaseType.MakeGenericType(args);
            }
            else
            {
                throw new InvalidOperationException($"Funcs with {parameters.Length} parameters are not supported");
            }
            return delegateType;
        }

        private static Type _GetActionDelegateType(Type[] parameters)
        {
            if (parameters.Length == 0)
            {
                return typeof(Action);
            }

            Type delegateType;
            Type openBaseType;
            if (DelegateMap.ActionsByParameterNumber.TryGetValue(parameters.Length, out openBaseType))
            {
                delegateType = openBaseType.MakeGenericType(parameters);
            }
            else
            {
                throw new InvalidOperationException($"Actions with {parameters.Length} parameters are not supported");
            }
            return delegateType;
        }

        private static void _EmitArguments(ILGenerator il, int length)
        {
            if (length == 0)
                return;
            for (int i = 0; i < length; i++)
                il.Emit(OpCodes.Ldarg, i);
        }

        private static bool _IsMethodVoid(MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        private static string _GetAnonymousMethodName()
        {
            return "DynamicEventHandler" + Guid.NewGuid().ToString("N");
        }
    }
}

