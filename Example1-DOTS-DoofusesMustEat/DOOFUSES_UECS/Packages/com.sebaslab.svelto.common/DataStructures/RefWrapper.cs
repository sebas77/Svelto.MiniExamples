using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Svelto.DataStructures
{
    public static class TypeRefWrapper<T>
    {
        public static RefWrapperType wrapper = new RefWrapperType(typeof(T));        
    }
    
    [DebuggerDisplay("{_type}")]
    public readonly struct RefWrapperType: IEquatable<RefWrapperType> 
    {
        public RefWrapperType(Type type)
        {
            _type     = type;
            _hashCode = type.GetHashCode();
        }

        public bool Equals(RefWrapperType other)
        {
            return _type == other._type;
        }
        
        public override int GetHashCode()
        {
            return _hashCode;
        }
        
        public static implicit operator Type(RefWrapperType t) => t._type;

        readonly          Type _type;
        internal readonly int  _hashCode;
    }
    
    public readonly struct NativeRefWrapperType: IEquatable<NativeRefWrapperType>
    {
        internal static FasterDictionary<RefWrapperType, System.Guid> GUIDCache =
            new FasterDictionary<RefWrapperType, Guid>();
        
        public NativeRefWrapperType(RefWrapperType type)
        {
            _type     = GUIDCache.GetOrAdd(type, (ref RefWrapperType type) => ((Type)type).GUID, ref type);
            _hashCode = type._hashCode;
        }

        public bool Equals(NativeRefWrapperType other)
        {
            return _type == other._type;
        }
        
        public override int GetHashCode()
        {
            return _hashCode;
        }
        
        readonly Guid _type;
        readonly int  _hashCode;
    }
    
    public readonly struct RefWrapper<T>: IEquatable<RefWrapper<T>>, IEquatable<T> where T:class
    {
        public RefWrapper(T type)
        {
            _type    = type;
            _hashCode = type.GetHashCode();
        }

        public bool Equals(RefWrapper<T> other)
        {
            return _type.Equals(other._type);
        }

        public bool Equals(T other)
        {
            return _type.Equals(other);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public T type => _type;

        public static implicit operator T(RefWrapper<T> t) => t._type;
        public static implicit operator RefWrapper<T>(T t) => new RefWrapper<T>(t);

        readonly T   _type;
        readonly int _hashCode;
    }
    
    public readonly struct RefWrapper<T, Comparer>: IEquatable<RefWrapper<T, Comparer>>, IEquatable<T> 
        where T:class where Comparer: struct, IEqualityComparer<T>
    {
        public RefWrapper(T type)
        {
            _type    = type;
            _comparer = default;
        }

        public bool Equals(RefWrapper<T, Comparer> other)
        {
            return _comparer.Equals(this, other.type);
        }

        public bool Equals(T other)
        {
            return _comparer.Equals(type, other);
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode(this);
        }

        public T type => _type;

        public static implicit operator T(RefWrapper<T, Comparer> t) => t.type;
        public static implicit operator RefWrapper<T, Comparer>(T t) => new RefWrapper<T, Comparer>(t);

        readonly T        _type;
        readonly Comparer _comparer;
    }
}