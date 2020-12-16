using System;

namespace Svelto.ECS
{
    public readonly ref struct DoubleEntitiesEnumerator<T1, T2, T3> where T1 : struct, IEntityComponent
                                                                    where T2 : struct, IEntityComponent
                                                                    where T3 : struct, IEntityComponent
    {
        public DoubleEntitiesEnumerator(GroupsEnumerable<T1, T2, T3> groupsEnumerable)
        {
            _groupsEnumerable = groupsEnumerable;
        }

        public EntityGroupsIterator GetEnumerator() { return new EntityGroupsIterator(_groupsEnumerable); }

        readonly GroupsEnumerable<T1, T2, T3> _groupsEnumerable;

        public ref struct EntityGroupsIterator
        {
            public EntityGroupsIterator(GroupsEnumerable<T1, T2, T3> groupsEnumerable) : this()
            {
                _groupsEnumerableA = groupsEnumerable.GetEnumerator();
                _groupsEnumerableA.MoveNext();
                _groupsEnumerableB = _groupsEnumerableA;
                _indexA            = 0;
                _indexB            = 0;
            }

            public bool MoveNext()
            {
                if (_groupsEnumerableA.isValid)
                {
                    var (buffersA, _) = _groupsEnumerableA.Current;
                    var (buffersB, _) = _groupsEnumerableB.Current;
                    if (_indexA < buffersA.count)
                    {
                        if (++_indexB < buffersB.count)
                        {
                            return true;
                        }

                        if (_groupsEnumerableB.MoveNext() == false)
                        {
                            _groupsEnumerableB = _groupsEnumerableA;
                        }

                        ++_indexA;
                        _indexB = _indexA;
                    }
                    else
                    {
                        if (_groupsEnumerableA.MoveNext() == false)
                            return false;
                        
                        _indexA = 0;
                        _indexB = 0;
                    }
                }

                return false;
            }

            public void Reset() { throw new Exception(); }

            public ValueRef Current
            {
                get
                {
                    var valueRef = new ValueRef(_groupsEnumerableA.Current, _indexA, _groupsEnumerableB.Current
                                              , _indexB);
                    return valueRef;
                }
            }

            public void Dispose() { }

            GroupsEnumerable<T1, T2, T3>.GroupsIterator _groupsEnumerableA;
            GroupsEnumerable<T1, T2, T3>.GroupsIterator _groupsEnumerableB;
            int                                         _indexA;
            int                                         _indexB;
        }

        public ref struct ValueRef
        {
            public readonly RefCurrent<T1, T2, T3> _current;
            public readonly int                    _indexA;
            public readonly RefCurrent<T1, T2, T3> _refCurrent;
            public readonly int                    _indexB;

            public ValueRef(RefCurrent<T1, T2, T3> current, int indexA, RefCurrent<T1, T2, T3> refCurrent, int indexB)
            {
                _current    = current;
                _indexA     = indexA;
                _refCurrent = refCurrent;
                _indexB     = indexB;
            }

            public void Deconstruct
            (out EntityCollection<T1, T2, T3> buffers, out int indexA, out RefCurrent<T1, T2, T3> refCurrent
           , out int indexB)
            {
                buffers    = _current._buffers;
                indexA     = _indexA;
                refCurrent = _refCurrent;
                indexB     = _indexB;
            }
        }
    }
}