using System.Runtime.InteropServices;

namespace Svelto.ECS.Hybrid
{
    /// <summary>
    /// an ECS dirty trick to hold the reference to an object. this is component can be used in an engine
    /// managing an OOP abstraction layer. It's need is quite rare though! All other uses must be considered an abuse
    /// as the object can be casted back to it's real type only by an OOP Abstraction Layer Engine:
    /// https://www.sebaslab.com/oop-abstraction-layer-in-a-ecs-centric-application/ 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ValueReference<T> where T:class, IImplementor
    {
        public ValueReference(T obj) { _pointer = GCHandle.Alloc(obj, GCHandleType.Normal); }

        public static explicit operator T(ValueReference<T> t) => (T) t._pointer.Target;

        GCHandle _pointer;
    }
}