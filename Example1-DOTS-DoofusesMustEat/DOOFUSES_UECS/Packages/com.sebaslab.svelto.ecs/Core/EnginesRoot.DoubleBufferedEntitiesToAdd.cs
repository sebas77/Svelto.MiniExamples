using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        internal class DoubleBufferedEntitiesToAdd
        {
            //while caching is good to avoid over creating dictionaries that may be reused, the side effect
            //is that I have to iterate every time up to 100 dictionaries during the flushing of the build entities
            //even if there are 0 entities inside.
            const int MAX_NUMBER_OF_GROUPS_TO_CACHE          = 100;
            const int MAX_NUMBER_OF_TYPES_PER_GROUP_TO_CACHE = 100;

            public DoubleBufferedEntitiesToAdd()
            {
                var entitiesCreatedPerGroupA = new FasterDictionary<ExclusiveGroupStruct, uint>();
                var entitiesCreatedPerGroupB = new FasterDictionary<ExclusiveGroupStruct, uint>();
                var entityComponentsToAddBufferA =
                    new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>();
                var entityComponentsToAddBufferB =
                    new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>();

                _currentNumberEntitiesCreatedPerGroup = entitiesCreatedPerGroupA;
                _otherNumberEntitiesCreatedPerGroup   = entitiesCreatedPerGroupB;

                currentComponentsToAddPerGroup = entityComponentsToAddBufferA;
                otherComponentsToAddPerGroup   = entityComponentsToAddBufferB;
            }

            public void ClearOther()
            {
                var numberOfGroupsAddedSoFar     = otherComponentsToAddPerGroup.count;
                var componentDictionariesPerType = otherComponentsToAddPerGroup.unsafeValues;

                //If we didn't create too many groups, we keep them alive, so we avoid the cost of creating new dictionaries
                //during future submissions, otherwise we clean up everything
                if (numberOfGroupsAddedSoFar > MAX_NUMBER_OF_GROUPS_TO_CACHE)
                {
                    for (var i = 0; i < numberOfGroupsAddedSoFar; ++i)
                    {
                        var componentTypesCount      = componentDictionariesPerType[i].count;
                        var componentTypesDictionary = componentDictionariesPerType[i].unsafeValues;
                        {
                            for (var j = 0; j < componentTypesCount; ++j)
                                //dictionaries of components may be native so they need to be disposed
                                //before the references are GCed
                                componentTypesDictionary[j].Dispose();
                        }
                    }

                    //reset the number of entities created so far
                    _otherNumberEntitiesCreatedPerGroup.FastClear();
                    otherComponentsToAddPerGroup.FastClear();

                    return;
                }

                for (var i = 0; i < numberOfGroupsAddedSoFar; ++i)
                {
                    var                   componentTypesCount      = componentDictionariesPerType[i].count;
                    ITypeSafeDictionary[] componentTypesDictionary = componentDictionariesPerType[i].unsafeValues;
                    for (var j = 0; j < componentTypesCount; ++j)
                        //clear the dictionary of entities created so far (it won't allocate though)
                        componentTypesDictionary[j].FastClear();

                    //if we didn't create too many component for this group, I reuse the component arrays
                    if (componentTypesCount <= MAX_NUMBER_OF_TYPES_PER_GROUP_TO_CACHE)
                    {
                        for (var j = 0; j < componentTypesCount; ++j)
                            componentTypesDictionary[j].FastClear();
                    }
                    else
                    {
                        //here I have to dispose, because I am actually clearing the reference of the dictionary
                        //with the next line.
                        for (var j = 0; j < componentTypesCount; ++j)
                            componentTypesDictionary[j].Dispose();

                        componentDictionariesPerType[i].FastClear();
                    }
                }

                //reset the number of entities created so far
                _otherNumberEntitiesCreatedPerGroup.FastClear();

                _totalEntitiesToAdd = 0;
            }

            public void Dispose()
            {
                {
                    var otherValuesArray = otherComponentsToAddPerGroup.unsafeValues;
                    for (var i = 0; i < otherComponentsToAddPerGroup.count; ++i)
                    {
                        int                   safeDictionariesCount = otherValuesArray[i].count;
                        ITypeSafeDictionary[] safeDictionaries      = otherValuesArray[i].unsafeValues;
                        //do not remove the dictionaries of entities per type created so far, they will be reused
                        for (var j = 0; j < safeDictionariesCount; ++j)
                            //clear the dictionary of entities create do far (it won't allocate though)
                            safeDictionaries[j].Dispose();
                    }
                }
                {
                    var currentValuesArray = currentComponentsToAddPerGroup.unsafeValues;
                    for (var i = 0; i < currentComponentsToAddPerGroup.count; ++i)
                    {
                        int                   safeDictionariesCount = currentValuesArray[i].count;
                        ITypeSafeDictionary[] safeDictionaries      = currentValuesArray[i].unsafeValues;
                        //do not remove the dictionaries of entities per type created so far, they will be reused
                        for (var j = 0; j < safeDictionariesCount; ++j)
                            //clear the dictionary of entities create do far (it won't allocate though)
                            safeDictionaries[j].Dispose();
                    }
                }

                _currentNumberEntitiesCreatedPerGroup = null;
                _otherNumberEntitiesCreatedPerGroup   = null;
                otherComponentsToAddPerGroup          = null;
                currentComponentsToAddPerGroup        = null;
            }

            internal bool AnyEntityCreated()
            {
                return _currentNumberEntitiesCreatedPerGroup.count > 0;
            }

            internal bool AnyOtherEntityCreated()
            {
                return _otherNumberEntitiesCreatedPerGroup.count > 0;
            }

            internal void IncrementEntityCount(ExclusiveGroupStruct groupID)
            {
                _currentNumberEntitiesCreatedPerGroup.GetOrCreate(groupID)++;
                _totalEntitiesToAdd++;
            }

            public uint NumberOfEntitiesToAdd()
            {
                return _totalEntitiesToAdd;
            }

            internal void Preallocate
                (ExclusiveGroupStruct groupID, uint numberOfEntities, IComponentBuilder[] entityComponentsToBuild)
            {
                void PreallocateDictionaries
                    (FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>> dic)
                {
                    var group = dic.GetOrCreate(
                        groupID, () => new FasterDictionary<RefWrapperType, ITypeSafeDictionary>());

                    foreach (var componentBuilder in entityComponentsToBuild)
                    {
                        var entityComponentType = componentBuilder.GetEntityComponentType();
                        var safeDictionary = group.GetOrCreate(new RefWrapperType(entityComponentType)
                                                             , () => componentBuilder
                                                                  .CreateDictionary(numberOfEntities));
                        componentBuilder.Preallocate(safeDictionary, numberOfEntities);
                    }
                }

                PreallocateDictionaries(currentComponentsToAddPerGroup);
                PreallocateDictionaries(otherComponentsToAddPerGroup);

                _currentNumberEntitiesCreatedPerGroup.GetOrCreate(groupID);
                _otherNumberEntitiesCreatedPerGroup.GetOrCreate(groupID);
            }

            internal void Swap()
            {
                Swap(ref currentComponentsToAddPerGroup, ref otherComponentsToAddPerGroup);
                Swap(ref _currentNumberEntitiesCreatedPerGroup, ref _otherNumberEntitiesCreatedPerGroup);
            }

            static void Swap<T>(ref T item1, ref T item2)
            {
                (item2, item1) = (item1, item2);
            }

            public OtherComponentsToAddPerGroupEnumerator GetEnumerator()
            {
                return new OtherComponentsToAddPerGroupEnumerator(otherComponentsToAddPerGroup
                                                                , _otherNumberEntitiesCreatedPerGroup);
            }

            //Before I tried for the third time to use a SparseSet instead of FasterDictionary, remember that
            //while group indices are sequential, they may not be used in a sequential order. Sparseset needs
            //entities to be created sequentially (the index cannot be managed externally)
            internal FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>
                currentComponentsToAddPerGroup;

            FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>
                otherComponentsToAddPerGroup;

            /// <summary>
            ///     To avoid extra allocation, I don't clear the groups, so I need an extra data structure
            ///     to keep count of the number of entities built this frame. At the moment the actual number
            ///     of entities built is not used
            /// </summary>
            FasterDictionary<ExclusiveGroupStruct, uint> _currentNumberEntitiesCreatedPerGroup;

            FasterDictionary<ExclusiveGroupStruct, uint> _otherNumberEntitiesCreatedPerGroup;

            uint _totalEntitiesToAdd;
        }
    }

    struct OtherComponentsToAddPerGroupEnumerator
    {
        public OtherComponentsToAddPerGroupEnumerator
        (FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>
             otherComponentsToAddPerGroup
       , FasterDictionary<ExclusiveGroupStruct, uint> otherNumberEntitiesCreatedPerGroup)
        {
            _otherComponentsToAddPerGroup       = otherComponentsToAddPerGroup.GetEnumerator();
            _otherNumberEntitiesCreatedPerGroup = otherNumberEntitiesCreatedPerGroup.GetEnumerator();
            Current                             = default;
        }

        public bool MoveNext()
        {
            while (_otherNumberEntitiesCreatedPerGroup.MoveNext())
            {
                var current = _otherNumberEntitiesCreatedPerGroup.Current;
                var ret     = _otherComponentsToAddPerGroup.MoveNext();

                DBC.ECS.Check.Assert(ret == true);

                if (current.Value > 0)
                {
                    var keyValuePairFast = _otherComponentsToAddPerGroup.Current;
                    Current = new GroupInfo()
                    {
                        group      = keyValuePairFast.Key
                      , components = keyValuePairFast.Value
                    };

                    return true;
                }
            }

            return false;
        }

        public GroupInfo Current { get; private set; }

        //cannot be read only as they will be modified by MoveNext
        SveltoDictionaryKeyValueEnumerator<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>,
                ManagedStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>,
                ManagedStrategy<FasterDictionary<RefWrapperType, ITypeSafeDictionary>>, ManagedStrategy<int>>
            _otherComponentsToAddPerGroup;

        SveltoDictionaryKeyValueEnumerator<ExclusiveGroupStruct, uint,
                ManagedStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>, ManagedStrategy<uint>,
                ManagedStrategy<int>>
            _otherNumberEntitiesCreatedPerGroup;
    }

    struct GroupInfo
    {
        public ExclusiveGroupStruct                                  group;
        public FasterDictionary<RefWrapperType, ITypeSafeDictionary> components;
    }
}