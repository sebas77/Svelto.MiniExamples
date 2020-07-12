using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    struct GroupsList
    {
        static GroupsList()
        {
            groups = new FasterList<ExclusiveGroupStruct>();
        }

        static readonly FasterList<ExclusiveGroupStruct> groups;
        public FasterList<ExclusiveGroupStruct> reference => groups;
    }
    
    public struct QueryGroups
    {
        static readonly ThreadLocal<GroupsList> groups = new ThreadLocal<GroupsList>();
        
        public FasterList<ExclusiveGroupStruct> result => groups.Value.reference;
        
        public QueryGroups(FasterDictionary<uint, ITypeSafeDictionary> findGroups)
        {
            var group = groups.Value.reference;
            group.FastClear();
            foreach (var keyvalue in findGroups)
            {
                group.Add(new ExclusiveGroupStruct(keyvalue.Key));
            }
        }

        public QueryGroups Except(ExclusiveGroupStruct[] groupsToIgnore)
        {
            var group = groups.Value.reference;
            var groupsCount = group.count;

            for (int i = 0; i < groupsToIgnore.Length; i++)
            {
                for (int j = 0; j < groupsCount; j++)
                    if (groupsToIgnore[i] == group[j])
                    {
                        group.UnorderedRemoveAt(j);
                        j--;
                        groupsCount--;
                    }
            }

            return this;
        }
    }
}