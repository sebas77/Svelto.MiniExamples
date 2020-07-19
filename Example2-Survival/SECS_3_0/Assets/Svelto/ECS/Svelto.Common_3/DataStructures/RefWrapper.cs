using System;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public static class TypeRefWrapper<T>
    {
        public static RefWrapper<Type> wrapper = new RefWrapper<Type>(typeof(T));        
    }
    
    public readonly struct RefWrapper<T>: IEquatable<RefWrapper<T>> where T:class
    {
        public RefWrapper(T obj)
        {
            _value = obj;
        }

        public bool Equals(RefWrapper<T> other)
        {
            return _value == other._value;
        }
        
        public override int GetHashCode()
        {
            return TypeHash<T>.hash;
        }
        
        public static implicit operator T(RefWrapper<T> t) => t._value;

        readonly T _value;
    }
}