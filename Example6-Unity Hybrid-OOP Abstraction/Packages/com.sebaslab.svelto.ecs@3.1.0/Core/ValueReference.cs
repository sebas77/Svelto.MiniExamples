using System;
using System.Runtime.InteropServices;

namespace Svelto.ECS
{
    public struct ValueReference<T>
    {
        public ValueReference(Object obj) { _pointer = GCHandle.Alloc(obj, GCHandleType.Normal); }

        public static implicit operator T(ValueReference<T> t) => (T) t._pointer.Target;

        GCHandle _pointer;
    }
}