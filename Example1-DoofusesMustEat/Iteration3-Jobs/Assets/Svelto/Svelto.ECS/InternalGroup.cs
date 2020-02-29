namespace Svelto.ECS.Internal
{
    public struct InternalGroup
    {
        readonly ExclusiveGroupStruct _group;
        internal InternalGroup(ExclusiveGroupStruct @group) { _group = group; }
        
        public static implicit operator ExclusiveGroupStruct(InternalGroup groupStruct)
        {
            return groupStruct._group;
        }
    }
}