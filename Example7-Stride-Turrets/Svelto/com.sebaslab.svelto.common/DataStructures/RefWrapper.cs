using System;
using System.Collections.Generic;

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

        public T value => _value;

        public static implicit operator T(RefWrapper<T> t) => t._value;
        public static implicit operator RefWrapper<T>(T t) => new RefWrapper<T>(t);

        readonly T   _value;
        readonly int _hashCode;
    }
    
    public readonly struct RefWrapper<T, Comparer>: IEquatable<RefWrapper<T, Comparer>>, IEquatable<T> where T:class
    where Comparer: struct, IEqualityComparer<T>
    {
        public RefWrapper(T obj)
        {
            _value    = obj;
            _hashCode = _value.GetHashCode();
            _comparer = default;
        }

        public bool Equals(RefWrapper<T, Comparer> other)
        {
            return _comparer.Equals(this, other.value);
        }

        public bool Equals(T other)
        {
            return _comparer.Equals(value, other);
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode(this);
        }

        public T value => _value;

        public static implicit operator T(RefWrapper<T, Comparer> t) => t.value;
        public static implicit operator RefWrapper<T, Comparer>(T t) => new RefWrapper<T, Comparer>(t);

        readonly T        _value;
        readonly int      _hashCode;
        readonly Comparer _comparer;
    }
}