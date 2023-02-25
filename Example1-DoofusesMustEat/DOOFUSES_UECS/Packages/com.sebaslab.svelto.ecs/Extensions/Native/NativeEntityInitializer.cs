#if UNITY_NATIVE //at the moment I am still considering NativeOperations useful only for Unity
using Svelto.DataStructures;

namespace Svelto.ECS.Native
{
    public readonly ref struct NativeEntityInitializer
    {
        readonly NativeBag _unsafeBuffer;
        readonly UnsafeArrayIndex _componentsToInitializeCounterRef;
        readonly EntityReference _reference;

        public NativeEntityInitializer(in NativeBag unsafeBuffer, UnsafeArrayIndex componentsToInitializeCounterRef, EntityReference reference)
        {
            _unsafeBuffer = unsafeBuffer;
            _componentsToInitializeCounterRef = componentsToInitializeCounterRef;
            _reference = reference;
        }

        public void Init<T>(in T component)
                where T : unmanaged, IEntityComponent
        {
            uint id = EntityComponentID<T>.ID.Data;

            _unsafeBuffer.AccessReserved<uint>(_componentsToInitializeCounterRef)++; //increase the number of components that have been initialised by the user

            //Since NativeEntityInitializer is a ref struct, it guarantees that I am enqueueing components of the
            //last entity built
            _unsafeBuffer.Enqueue(id);
            _unsafeBuffer.Enqueue(component);
        }

        public EntityReference reference => _reference;
    }
}
#endif