using System.Runtime.InteropServices;

namespace Svelto.ECS.Hybrid
{
    /// <summary>
    /// an ECS dirty trick to hold the reference to an object. this is component can be used in an engine
    /// managing an OOP abstraction layer. It's need is quite rare though! 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ValueReference<T> where T:class
    {
        public ValueReference(T obj) { _pointer = GCHandle.Alloc(obj, GCHandleType.Normal); }

        public T ConvertAndDispose<W>(W implementer) where W:IImplementor 
        {
            var pointerTarget = _pointer.Target;
            _pointer.Free();
            return (T)pointerTarget;
        }

        public bool isDefault => _pointer.IsAllocated == false;
        
        GCHandle    _pointer;
    }
}