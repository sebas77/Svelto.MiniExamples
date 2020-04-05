using System;

namespace Svelto.Common
{
    public class TypeCache<T>
    {
        public static readonly Type type = typeof(T);
    }
}