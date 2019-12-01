using System;

namespace Svelto.DataStructures
{
    public struct RefWrapper<T>: IEquatable<RefWrapper<T>> where T:class
    {
        public RefWrapper(T obj)
        {
            _value = obj;
        }

        public bool Equals(RefWrapper<T> other)
        {
            return _value.Equals(other._value);
        }
        
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
        
        public static implicit operator T(RefWrapper<T> t) => t._value;

        readonly T _value;
    }
}