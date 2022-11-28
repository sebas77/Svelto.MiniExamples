using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.DataStructures.Native;

namespace Svelto.ECS.Resources
{
    /// <summary>
    ///     Inherit this class to have the base functionalities to implement a custom ECS compatible resource manager
    /// </summary>
    public class ECSResourceManager<T> where T : class
    {
        protected ECSResourceManager()
        {
            _sparse = new ValueContainer<T, ManagedStrategy<T>, NativeStrategy<SparseIndex>>(16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ValueIndex Add(in T resource)
        {
            return _sparse.Add(resource);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Remove(ValueIndex index)
        {
            _sparse.Remove(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _sparse.Clear();
        }

        public T this[ValueIndex index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sparse[index];
        }
        
        readonly ValueContainer<T, ManagedStrategy<T>, NativeStrategy<SparseIndex>> _sparse;
    }
}