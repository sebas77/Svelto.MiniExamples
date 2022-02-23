using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Svelto.DataStructures.Native;
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public struct EntityFilterCollection
    {
        internal static EntityFilterCollection Create()
        {
            var collection = new EntityFilterCollection
            {
                _filtersPerGroup = SharedSveltoDictionaryNative<ExclusiveGroupStruct, GroupFilters>.Create()
            };
            return collection;
        }

        public EntityFilterIterator GetEnumerator() => new EntityFilterIterator(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(EGID egid, NativeEGIDMapper<T> mmap) where T : unmanaged, IEntityComponent
        {
            Add(egid, mmap.GetIndex(egid.entityID));
        }
        
        public void Add<T>(EGID egid, NativeEGIDMultiMapper<T> mmap) where T : unmanaged, IEntityComponent
        {
            Add(egid, mmap.GetIndex(egid));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(EGID egid, uint toIndex)
        {
            GetGroupFilter(egid.groupID).Add(egid.entityID, toIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GroupFilters GetGroupFilter(ExclusiveBuildGroup group)
        {
            if (_filtersPerGroup.TryGetValue(group, out var groupFilter) == false)
            {
                groupFilter = new GroupFilters(group);
                _filtersPerGroup.Add(group, groupFilter);
            }

            return groupFilter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EGID egid)
        {
            _filtersPerGroup[egid.groupID].Remove(egid.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists()
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var filterSets = _filtersPerGroup.GetValues(out var count);
            for (var i = 0; i < count; i++)
            {
                filterSets[i].Clear();
            }
        }

        internal int groupCount => _filtersPerGroup.count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal GroupFilters GetGroup(int indexGroup)
        {
            DBC.ECS.Check.Require(indexGroup < _filtersPerGroup.count);
            return _filtersPerGroup.GetValues(out _)[indexGroup];
        }

        internal void Dispose()
        {
            var filterSets = _filtersPerGroup.GetValues(out var count);
            for (var i = 0; i < count; i++)
            {
                filterSets[i].Dispose();
            }
            _filtersPerGroup.Dispose();
        }

        //double check if this needs to be shared
        internal SharedSveltoDictionaryNative<ExclusiveGroupStruct, GroupFilters> _filtersPerGroup;

        public struct GroupFilters
        {
            internal GroupFilters(ExclusiveGroupStruct group)
            {
                _entityIDToDenseIndex = new SharedSveltoDictionaryNative<uint, uint>(1);
                _indexToEntityId      = new SharedSveltoDictionaryNative<uint, uint>(1);
                _group                = group;
            }

            public void Add(uint entityId, uint entityIndex)
            {
                _entityIDToDenseIndex.Add(entityId, entityIndex);
                _indexToEntityId.Add(entityIndex, entityId);
            }

            public void Remove(uint entityId)
            {
                _indexToEntityId.Remove(_entityIDToDenseIndex[entityId]);
                _entityIDToDenseIndex.Remove(entityId);
            }

            internal void RemoveWithSwapBack(uint entityId, uint entityIndex, uint lastIndex)
            {
                // Check if the last index is part of the filter as an entity, in that case
                //we need to update the filter
                if (entityIndex != lastIndex && _indexToEntityId.TryGetValue(lastIndex, out var lastEntityID))
                {
                    _entityIDToDenseIndex[lastEntityID] = entityIndex;
                    _indexToEntityId[entityIndex]       = lastEntityID;

                    _indexToEntityId.Remove(lastIndex);
                }
                else
                {
                    // We don't need to check if the entityIndex is a part of the dictionary.
                    // The Remove function will check for us.
                    _indexToEntityId.Remove(entityIndex);
                }

                // We don't need to check if the entityID is part of the dictionary.
                // The Remove function will check for us.
                _entityIDToDenseIndex.Remove(entityId);
            }

            internal void Clear()
            {
                _indexToEntityId.FastClear();
                _entityIDToDenseIndex.FastClear();
            }

            public bool Exists(uint entityId) => _entityIDToDenseIndex.ContainsKey(entityId);

            internal void Dispose()
            {
                _entityIDToDenseIndex.Dispose();
                _indexToEntityId.Dispose();
            }

            internal EntityFilterIndices indices
            {
                get
                {
                    var values = _entityIDToDenseIndex.GetValues(out var count);
                    return new EntityFilterIndices(values, count);
                }
            }

            public uint count => (uint)_entityIDToDenseIndex.count;

            internal ExclusiveGroupStruct group   => _group;
            public   bool                 isValid => _entityIDToDenseIndex.isValid;

            SharedSveltoDictionaryNative<uint, uint>          _indexToEntityId;
            internal SharedSveltoDictionaryNative<uint, uint> _entityIDToDenseIndex;
            readonly ExclusiveGroupStruct                     _group;
        }
    }
}