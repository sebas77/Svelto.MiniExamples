using System;
using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;


namespace Svelto.ECS
{
    public struct FilterContextID
    {
        public readonly uint id;

        internal FilterContextID(uint id)
        {
            DBC.ECS.Check.Require(id < ushort.MaxValue, "too many types registered, HOW :)");

            this.id = id;
        }
    }

    public struct CombinedFilterID
    {
        public readonly long id;

        public CombinedFilterID(int filterID, FilterContextID contextID)
        {
            id = (long)filterID << 32 | (uint)contextID.id << 16;
        }

        public static implicit operator CombinedFilterID((int filterID, FilterContextID contextID) data)
        {
            return new CombinedFilterID(data.filterID, data.contextID);
        }
    }
    
    //this cannot be inside EntitiesDB otherwise it will cause hashing of reference in Burst
    public class Internal_FilterHelper
    {
        //since the user can choose their own filterID, in order to avoid collisions between
        //filters of the same type, the FilterContext is provided. The type is identified through
        //TypeCounter
        public static long CombineFilterIDs<T>(CombinedFilterID combinedFilterID) where T: struct, IEntityComponent
        {
            var id = (uint)ComponentID<T>.id.Data;

            var combineFilterIDs = (long)combinedFilterID.id | id;

            return combineFilterIDs;
        }
    }

    public partial class EntitiesDB
    {
        public SveltoFilters GetFilters()
        {
            return new SveltoFilters(_enginesRoot._persistentEntityFilters,
                _enginesRoot._indicesOfPersistentFiltersUsedByThisComponent, _enginesRoot._transientEntityFilters);
        }

        /// <summary>
        /// this whole structure is usable inside DOTS JOBS and BURST
        /// </summary>
        public readonly struct SveltoFilters
        {
            static readonly SharedStaticWrapper<int, Internal_FilterHelper> uniqueContextID =
                new SharedStaticWrapper<int, Internal_FilterHelper>(1);
            
            public static FilterContextID GetNewContextID()
            {
                return new FilterContextID((uint)Interlocked.Increment(ref uniqueContextID.Data));
            }

            public SveltoFilters(SharedSveltoDictionaryNative<long, EntityFilterCollection> persistentEntityFilters,
                SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>
                    indicesOfPersistentFiltersUsedByThisComponent,
                SharedSveltoDictionaryNative<long, EntityFilterCollection> transientEntityFilters)
            {
                _persistentEntityFilters                       = persistentEntityFilters;
                _indicesOfPersistentFiltersUsedByThisComponent = indicesOfPersistentFiltersUsedByThisComponent;
                _transientEntityFilters                        = transientEntityFilters;
            }
            
#if UNITY_BURST
            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(int filterID,
                FilterContextID filterContextId, NativeRefWrapperType typeRef) where T : unmanaged, IEntityComponent
            {
                return ref GetOrCreatePersistentFilter<T>(new CombinedFilterID(filterID, filterContextId), typeRef);
            }

            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(CombinedFilterID filterID,
                NativeRefWrapperType typeRef) where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(filterID);
                
                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref _persistentEntityFilters.GetDirectValueByRef(index);

                _persistentEntityFilters.Add(combineFilterIDs, EntityFilterCollection.Create());

                var lastIndex = _persistentEntityFilters.count - 1;

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

                return ref _persistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }
#endif

            /// <summary>
            /// Create a persistent filter. Persistent filters are not deleted after each submission,
            /// however they have a maintenance cost that must be taken into account and will affect
            /// entities submission performance.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
#if UNITY_BURST && UNITY_COLLECTIONS
            [Unity.Collections.NotBurstCompatible]
#endif
            public EntityFilterCollection GetOrCreatePersistentFilter<T>(int filterID, FilterContextID filterContextId)
                where T : unmanaged, IEntityComponent
            {
                return GetOrCreatePersistentFilter<T>(new CombinedFilterID(filterID, filterContextId));
            }
#if UNITY_BURST && UNITY_COLLECTIONS
            [Unity.Collections.NotBurstCompatible]
#endif
            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(filterID);
                
                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref _persistentEntityFilters.GetDirectValueByRef(index);

                var typeRef          = TypeRefWrapper<T>.wrapper;
                var filterCollection = EntityFilterCollection.Create();

                _persistentEntityFilters.Add(combineFilterIDs, filterCollection);

                var lastIndex = _persistentEntityFilters.count - 1;

                _indicesOfPersistentFiltersUsedByThisComponent.GetOrAdd(new NativeRefWrapperType(typeRef),
                    () => new NativeDynamicArrayCast<int>(1, Svelto.Common.Allocator.Persistent)).Add(lastIndex);

                return ref _persistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }

            public EntityFilterCollection GetPersistentFilter<T>(int filterID, FilterContextID filterContextId)
                where T : unmanaged, IEntityComponent
            {
                return GetPersistentFilter<T>(new CombinedFilterID(filterID, filterContextId));
            }

            public ref EntityFilterCollection GetPersistentFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(filterID);
                
                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref _persistentEntityFilters.GetDirectValueByRef(index);

                throw new Exception("filter not found");
            }
            
            public bool TryGetPersistentFilter<T>(CombinedFilterID combinedFilterID, out EntityFilterCollection entityCollection) where T : struct, IEntityComponent
            {
                long combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(combinedFilterID);
                
                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                {
                    entityCollection = _persistentEntityFilters.GetDirectValueByRef(index);
                    return true;
                }

                entityCollection = default;
                return false;
            }

            public EntityFilterCollectionEnumerator GetPersistentFilters<T>() where T : unmanaged, IEntityComponent
            {
                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(
                        new NativeRefWrapperType(new RefWrapperType(typeof(T))), out var index) == true)
                    return new EntityFilterCollectionEnumerator(
                        _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(index),
                        _persistentEntityFilters);

                throw new Exception($"no filters associated with the type {TypeCache<T>.name}");
            }

            public struct EntityFilterCollectionEnumerator
            {
                public EntityFilterCollectionEnumerator(NativeDynamicArrayCast<int> getDirectValueByRef,
                    SharedSveltoDictionaryNative<long, EntityFilterCollection> sharedSveltoDictionaryNative) : this()
                {
                    _getDirectValueByRef          = getDirectValueByRef;
                    _sharedSveltoDictionaryNative = sharedSveltoDictionaryNative;
                }

                public EntityFilterCollectionEnumerator GetEnumerator()
                {
                    return this;
                }

                public bool MoveNext()
                {
                    if (_currentIndex++ < _getDirectValueByRef.count)
                        return true;

                    return false;
                }

                public ref EntityFilterCollection Current =>
                    ref _sharedSveltoDictionaryNative.GetDirectValueByRef((uint)_currentIndex - 1);

                readonly NativeDynamicArrayCast<int>                                _getDirectValueByRef;
                readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _sharedSveltoDictionaryNative;
                int                                                                 _currentIndex;
            }

            /// <summary>
            /// Creates a transient filter. Transient filters are deleted after each submission
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ref EntityFilterCollection GetOrCreateTransientFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                var combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(filterID);

                if (_transientEntityFilters.TryFindIndex(combineFilterIDs, out var index))
                    return ref _transientEntityFilters.GetDirectValueByRef(index);

                var filterCollection = EntityFilterCollection.Create();

                _transientEntityFilters.Add(combineFilterIDs, filterCollection);

                return ref _transientEntityFilters.GetDirectValueByRef((uint)(_transientEntityFilters.count - 1));
            }

            readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _persistentEntityFilters;

            readonly SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>
                _indicesOfPersistentFiltersUsedByThisComponent;

            readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _transientEntityFilters;
        }
    }
}