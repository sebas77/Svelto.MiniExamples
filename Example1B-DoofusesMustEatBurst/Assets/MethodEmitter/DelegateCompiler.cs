using System;
using System.Reflection;

namespace MethodEmitter
{
    public static class DelegateCompiler
    {
        public static T Compile<T>(MethodInfo method)
        {
            if (typeof(Delegate).IsAssignableFrom(typeof(T)))
                return (T)(object)_Compile(method, typeof(T));
            throw new InvalidOperationException("Only delegate types are supported");
        }

        private static Delegate _Compile(MethodInfo method, Type type)
        {
            Delegate target = Delegate.CreateDelegate(type, method);
            return target;
        }
    }
}
