using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        internal static long CombineFilterIDs<T>(int filterID) => (long)filterID << 32 | TypeHash<T>.hash;

        /// <summary>
        /// Creates a transient filter. Transient filters are deleted after each submission
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void GetOrCreateTransientFilter<T>(int filterID) where T : IEntityComponent
        {
            var typeRef          = TypeRefWrapper<T>.wrapper;
            var filterCollection = new EntityFilterCollection(typeRef, this);

            _transientEntityFilters.Add(CombineFilterIDs<T>(filterID), filterCollection);
            _transientFilters.Add(filterCollection);
        }

        /// <summary>
        /// Create a persistent filter. Persistent filters are not deleted after each submission,
        /// however they have a maintenance cost that must be taken into account and will affect
        /// entities submission performance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void GetOrCreatePersistentFilter<T>(int filterID) where T : IEntityComponent
        {
            var typeRef          = TypeRefWrapper<T>.wrapper;
            var filterCollection = new EntityFilterCollection(typeRef, this);

            _persistentEntityFilters.Add(CombineFilterIDs<T>(filterID), filterCollection);
            _persistentFilters.Add(filterCollection);

            _persistentFiltersIndicesPerComponent.GetOrCreate(typeRef, () => new FasterList<int>())
               .Add(_persistentFilters.count - 1);
        }
        
        // /// <summary>
        // /// Creates a transient filter. Transient filters are deleted after each submission
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <returns></returns>
        // public void GetOrCreateNativeTransientFilter<T>(int filterID, NativeEGIDMultiMapper<T> mmap)
        //     where T : unmanaged, IEntityComponent
        // {
        //     var typeRef          = TypeRefWrapper<T>.wrapper;
        //     var filterCollection = new NativeEntityFilterCollection<T>(mmap);
        //
        //     _transientEntityFilters.Add(CombineFilterIDs<T>(filterID), filterCollection);
        //     _transientFilters.Add(filterCollection);
        // }
        //
        // /// <summary>
        // /// Create a persistent filter. Persistent filters are not deleted after each submission,
        // /// however they have a maintenance cost that must be taken into account and will affect
        // /// entities submission performance.
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <returns></returns>
        // public void GetOrCreateNativePersistentFilter<T>(int filterID, NativeEGIDMultiMapper<T> mmap)
        //     where T : unmanaged, IEntityComponent
        // {
        //     var typeRef          = TypeRefWrapper<T>.wrapper;
        //     var filterCollection = new NativeEntityFilterCollection<T>(mmap);
        //
        //     _persistentEntityFilters.Add(CombineFilterIDs<T>(filterID), filterCollection);
        //     _persistentFilters.Add(filterCollection);
        //
        //     _persistentFiltersIndicesPerComponent.GetOrCreate(typeRef, () => new FasterList<int>())
        //        .Add(_persistentFilters.count - 1);
        // }

        void InitFilters()
        {
            _transientEntityFilters               = new FasterDictionary<long, EntityFilterCollection>();
            _persistentEntityFilters              = new FasterDictionary<long, EntityFilterCollection>();
            _transientFilters                     = new FasterList<EntityFilterCollection>();
            _persistentFilters                    = new FasterList<EntityFilterCollection>();
            _persistentFiltersIndicesPerComponent = new FasterDictionary<RefWrapperType, FasterList<int>>();
        }

        void DisposeFilters()
        {
            foreach (var filter in _transientEntityFilters)
            {
                filter.value.Dispose();
            }

            foreach (var filter in _persistentEntityFilters)
            {
                filter.value.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        void ClearTransientFilters()
        {
            foreach (var filter in _transientFilters)
            {
                filter.Clear();
            }
        }

        void RemoveEntityFromPersistentFilters(EGID @from, RefWrapperType refWrapperType, ITypeSafeDictionary fromDic)
        {
            if (_persistentFiltersIndicesPerComponent.TryGetValue(refWrapperType, out var list))
            {
                var fromIndex = fromDic.GetIndex(from.entityID);
                var lastIndex = (uint)fromDic.count - 1;

                var listCount = list.count;
                for (int i = 0; i < listCount; ++i)
                {
                    if (_persistentFilters[i]._filtersPerGroup.TryGetValue(from.groupID, out var groupFilter))
                    {
                        groupFilter.RemoveWithSwapBack(from.entityID, fromIndex, lastIndex);
                    }
                }
            }
        }

        void SwapEntityBetweenPersistentFilters(FasterList<(uint, uint, string)> infosToProcess,
            FasterDictionary<uint, uint> fromIndices, ITypeSafeDictionary toComponentsDictionary,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup, uint lastIndex, FasterList<int> listOfFilters)
        {
            var listCount = listOfFilters.count;
            foreach (var (fromEntityID, toEntityID, _) in infosToProcess)
            {
                var fromIndex = fromIndices[fromEntityID];
                var toIndex   = toComponentsDictionary.GetIndex(toEntityID);
                var @from     = new EGID(fromEntityID, fromGroup);
                var to        = new EGID(toEntityID, toGroup);

                for (int i = 0; i < listCount; ++i)
                {
                    //if the group has a filter linked:
                    var persistentFilter = _persistentFilters[i];
                    if (persistentFilter._filtersPerGroup.TryGetValue(@from.groupID, out var groupFilter))
                    {
                        if (groupFilter.HasEntity(fromEntityID) == true)
                        {
                            persistentFilter.AddEntity(to, toIndex);
                        }

                        // Removing an entity from the diction`ary may affect the index of the last entity in the
                        // values dictionary array, so we need to to update the indices of the affected entities.
                        //must be outside because from may not be present in the filter, but last index is
                        groupFilter.RemoveWithSwapBack(@from.entityID, fromIndex, lastIndex);
                    }
                }
            }
        }

        internal FasterDictionary<long, EntityFilterCollection> _transientEntityFilters;
        internal FasterDictionary<long, EntityFilterCollection> _persistentEntityFilters;
        
        // internal FasterDictionary<long, NativeEntityFilterCollection> _transientEntityFilters;
        // internal FasterDictionary<long, NativeEntityFilterCollection> _persistentEntityFilters;

        FasterList<EntityFilterCollection>                _transientFilters;
        FasterList<EntityFilterCollection>                _persistentFilters;
        FasterDictionary<RefWrapperType, FasterList<int>> _persistentFiltersIndicesPerComponent;
    }
}