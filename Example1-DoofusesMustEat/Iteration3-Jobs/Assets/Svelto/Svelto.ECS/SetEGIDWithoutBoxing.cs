using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Svelto.ECS.Internal
{
    public delegate void SetEGIDWithoutBoxingActionCast<T>(ref T target, EGID egid) where T : struct, IEntityComponent;
    
    static class SetEGIDWithoutBoxing<T> where T : struct, IEntityComponent
    {
        public static readonly SetEGIDWithoutBoxingActionCast<T> SetIDWithoutBoxing = MakeSetter();

        static SetEGIDWithoutBoxingActionCast<T> MakeSetter()
        {
            if (ComponentBuilder<T>.HAS_EGID)
            {
#if !ENABLE_IL2CPP
//not working in .net standard 2.0 on client, bug?
//it may work with IL2CPP now: https://forum.unity.com/threads/are-c-expression-trees-or-ilgenerator-allowed-on-ios.489498/
                Type         myTypeA     = typeof(T);
                PropertyInfo myFieldInfo = myTypeA.GetProperty("ID");

                ParameterExpression targetExp = Expression.Parameter(typeof(T).MakeByRefType(), "target");
                ParameterExpression valueExp  = Expression.Parameter(typeof(EGID), "value");
                MemberExpression    fieldExp  = Expression.Property(targetExp, myFieldInfo);
                BinaryExpression    assignExp = Expression.Assign(fieldExp, valueExp);

                var setter = Expression.Lambda<SetEGIDWithoutBoxingActionCast<T>>(assignExp, targetExp, valueExp).Compile();

                return setter;
#else        
                return (ref T target, EGID value) =>
                       {
                           var needEgid = (target as INeedEGID);
                           needEgid.ID = value;
                           target      = (T) needEgid;
                       };
#endif
            }

            return null;
        }

        public static void Warmup()
        {         
        }
    }
}