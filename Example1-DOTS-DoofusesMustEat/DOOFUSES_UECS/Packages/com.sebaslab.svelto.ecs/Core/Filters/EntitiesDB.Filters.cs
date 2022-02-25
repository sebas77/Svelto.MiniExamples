using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        public SveltoFilters GetFilters()
        {
            return new SveltoFilters(_enginesRoot);
        }

        /// <summary>
        /// this whole structure is usable inside DOTS JOBS and BURST
        /// </summary>
        public readonly struct SveltoFilters
        {
            public struct CombinedFilterID
            {
                public readonly long id;

                public CombinedFilterID(int filterID, ContextID contextID)
                {
                    id = (long)filterID << 32 | (uint)contextID.id << 16;
                }
            }

            public struct ContextID
            {
                public readonly uint id;

                public ContextID(uint id)
                {
                    DBC.ECS.Check.Require(id < ushort.MaxValue, "too many types registered, HOW :)");

                    this.id = id;
                }
            }

            public static ContextID GetNewContextID()
            {
                return new ContextID((uint)Interlocked.Increment(ref uniqueContextID.Data));
            }

            static readonly SharedStatic<int, SveltoFilters> uniqueContextID = new SharedStatic<int, SveltoFilters>(1);

            readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _persistentEntityFilters;

            readonly SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>
                _indicesOfPersistentFiltersUsedByThisComponent;

            readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _transientEntityFilters;

            public SveltoFilters(EnginesRoot enginesRoot)
            {
                _persistentEntityFilters = enginesRoot._persistentEntityFilters;
                _indicesOfPersistentFiltersUsedByThisComponent =
                    enginesRoot._indicesOfPersistentFiltersUsedByThisComponent;
                _transientEntityFilters = enginesRoot._transientEntityFilters;
            }

            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(CombinedFilterID filterID,
                NativeRefWrapperType typeRef) where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs                   = EnginesRoot.CombineFilterIDs<T>(filterID);
                var  enginesRootPersistentEntityFilters = _persistentEntityFilters;

                if (enginesRootPersistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref enginesRootPersistentEntityFilters.GetDirectValueByRef(index);

                var filterCollection = EntityFilterCollection.Create();

                enginesRootPersistentEntityFilters.Add(combineFilterIDs, filterCollection);

                var lastIndex = enginesRootPersistentEntityFilters.count - 1;

                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(typeRef, out var getIndex) == false)
                {
                    var newArray = new NativeDynamicArrayCast<int>(1, Allocator.Persistent);
                    newArray.Add(lastIndex);
                    _indicesOfPersistentFiltersUsedByThisComponent.Add(typeRef, newArray);
                }
                else
                {
                    ref var array = ref _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(getIndex);
                    array.Add(lastIndex);
                }

                return ref enginesRootPersistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }

            /// <summary>
            /// Create a persistent filter. Persistent filters are not deleted after each submission,
            /// however they have a maintenance cost that must be taken into account and will affect
            /// entities submission performance.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs                   = EnginesRoot.CombineFilterIDs<T>(filterID);
                var  enginesRootPersistentEntityFilters = _persistentEntityFilters;

                if (enginesRootPersistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref enginesRootPersistentEntityFilters.GetDirectValueByRef(index);

                var typeRef          = TypeRefWrapper<T>.wrapper;
                var filterCollection = EntityFilterCollection.Create();

                enginesRootPersistentEntityFilters.Add(combineFilterIDs, filterCollection);

                var lastIndex = enginesRootPersistentEntityFilters.count - 1;

                _indicesOfPersistentFiltersUsedByThisComponent.GetOrAdd(new NativeRefWrapperType(typeRef),
                    () => new NativeDynamicArrayCast<int>(1, Allocator.Persistent)).Add(lastIndex);

                return ref enginesRootPersistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }

            /// <summary>
            /// Creates a transient filter. Transient filters are deleted after each submission
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ref EntityFilterCollection GetOrCreateTransientFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                var combineFilterIDs                  = EnginesRoot.CombineFilterIDs<T>(filterID);
                var enginesRootTransientEntityFilters = _transientEntityFilters;

                if (enginesRootTransientEntityFilters.TryFindIndex(combineFilterIDs, out var index))
                    return ref enginesRootTransientEntityFilters.GetDirectValueByRef(index);

                var filterCollection = EntityFilterCollection.Create();

                enginesRootTransientEntityFilters.Add(combineFilterIDs, filterCollection);

                return ref enginesRootTransientEntityFilters.GetDirectValueByRef(
                    (uint)(enginesRootTransientEntityFilters.count - 1));
            }
        }
    }
}