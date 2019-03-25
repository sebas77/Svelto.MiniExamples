using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MethodEmitter
{
    public static class MethodExpressionBuilder
    {
        public static T Compile<T>(MethodInfo method)
        {
            var parameters = method.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray();
            var parameterExpressions = method.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
            var body = Expression.Call(method, parameterExpressions);

            var funcType = _IsMethodVoid(method)
                ? _GetActionDelegateType(parameters)
                : _GetFuncDelegateType(method, parameters);

            return (T)(object)Expression.Lambda(funcType, body, parameterExpressions).Compile();
        }

        private static Type _GetFuncDelegateType(MethodInfo method, Type[] parameters)
        {
            Type funcType;
            var args = parameters.Concat(new[] {method.ReturnType}).ToArray();
            Type openBaseType;
            if (DelegateMap.FuncsByParameterNumber.TryGetValue(args.Length, out openBaseType))
            {
                funcType = openBaseType.MakeGenericType(args);
            }
            else
            {
                throw new InvalidOperationException($"Funcs with {parameters.Length} parameters are not supported");
            }
            return funcType;
        }

        private static Type _GetActionDelegateType(Type[] parameters)
        {
            if (parameters.Length == 0)
                return typeof(Action);

            Type funcType;
            Type openBaseType;
            if (DelegateMap.ActionsByParameterNumber.TryGetValue(parameters.Length, out openBaseType))
            {
                funcType = openBaseType.MakeGenericType(parameters);
            }
            else
            {
                throw new InvalidOperationException($"Actions with {parameters.Length} parameters are not supported");
            }
            return funcType;
        }

        private static bool _IsMethodVoid(MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }
    }
}
