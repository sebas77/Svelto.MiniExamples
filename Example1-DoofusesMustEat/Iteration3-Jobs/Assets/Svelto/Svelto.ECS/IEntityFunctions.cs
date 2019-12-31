using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.ECS
{
    public interface IEntityFunctions
    {
        //being entity ID globally not unique, the group must be specified when
        //an entity is removed. Not specifying the group will attempt to remove
        //the entity from the special standard group.
        void RemoveEntity<T>(uint entityID, ExclusiveGroup.ExclusiveGroupStruct  groupID) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(EGID entityegid) where T : IEntityDescriptor, new();
        
        void RemoveGroupAndEntities(ExclusiveGroup.ExclusiveGroupStruct groupID);

        void SwapEntitiesInGroup<T>(ExclusiveGroup.ExclusiveGroupStruct fromGroupID,
                                    ExclusiveGroup.ExclusiveGroupStruct toGroupID);
        
        void SwapEntityGroup<T>(uint entityID, ExclusiveGroup.ExclusiveGroupStruct fromGroupID, ExclusiveGroup.ExclusiveGroupStruct  toGroupID) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID fromID, ExclusiveGroup.ExclusiveGroupStruct toGroupID) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID fromID, ExclusiveGroup.ExclusiveGroupStruct toGroupID, ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup) where T : IEntityDescriptor, new();
        
        void SwapEntityGroup<T>(EGID fromID, EGID toId) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID fromID, EGID toId, ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup) where T : IEntityDescriptor, new();
        
        GenericEntityFunctionWrapper Pin();
    }

    public struct GenericEntityFunctionWrapper:IDisposable
    {
        GCHandle handle;

        internal GenericEntityFunctionWrapper(in IEntityFunctions genericEntityFunctions)
        {
            handle = GCHandle.Alloc(genericEntityFunctions, GCHandleType.Pinned);
        }

        public void Dispose() { handle.Free(); }
        public IEntityFunctions ToStruct()
        {
            unsafe
            {
                return Unsafe.AsRef<IEntityFunctions>((void*)handle.AddrOfPinnedObject());
            }
        }
    }
}