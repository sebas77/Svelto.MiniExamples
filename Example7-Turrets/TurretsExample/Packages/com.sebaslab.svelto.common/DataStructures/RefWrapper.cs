using System;

namespace Svelto.DataStructures
{
    public static class TypeRefWrapper<T>
    {
        public static RefWrapperType wrapper = new RefWrapperType(typeof(T));        
    }

    public readonly struct RefWrapperType: IEquatable<RefWrapperType> 
    {
        public RefWrapperType(Type obj)
        {
            _value    = obj;
            _hashCode = _value.GetHashCode();
        }

        public bool Equals(RefWrapperType other)
        {
            return _value == other._value;
        }
        
        public override int GetHashCode()
        {
            return _hashCode;
        }
        
        public static implicit operator Type(RefWrapperType t) => t._value;

        readonly Type _value;
        readonly int  _hashCode;
    }
    
    public readonly struct RefWrapper<T>: IEquatable<RefWrapper<T>>, IEquatable<T> where T:class
    {
        public RefWrapper(T obj)
        {
            _value    = obj;
            _hashCode = _value.GetHashCode();
        }

        public bool Equals(RefWrapper<T> other)
        {
            return _value.Equals(other._value);
        }

        public bool Equals(T other)
        {
            return _value.Equals(other);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public static implicit operator T(RefWrapper<T> t) => t._value;
        public static implicit operator RefWrapper<T>(T t) => new RefWrapper<T>(t);

        readonly T   _value;
        readonly int _hashCode;
    }
}