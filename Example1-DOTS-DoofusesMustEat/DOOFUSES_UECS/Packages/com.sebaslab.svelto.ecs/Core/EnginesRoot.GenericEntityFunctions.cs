using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class GenericEntityFunctions : IEntityFunctions
        {
            internal GenericEntityFunctions(EnginesRoot weakReference)
            {
                _enginesRoot = new WeakReference<EnginesRoot>(weakReference);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(uint entityID, ExclusiveBuildGroup groupID) where T :
                IEntityDescriptor, new()
            {
                RemoveEntity<T>(new EGID(entityID, groupID));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(EGID entityEGID, [CallerMemberName] string caller = null) where T : IEntityDescriptor, new()
            {
                DBC.ECS.Check.Require(entityEGID.groupID.isInvalid == false, "invalid group detected");
                var descriptorComponentsToBuild = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;
                _enginesRoot.Target.CheckRemoveEntityID(entityEGID, TypeCache<T>.type, caller);

                _enginesRoot.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityEGID, entityEGID,
                        descriptorComponentsToBuild, caller));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntitiesFromGroup(ExclusiveBuildGroup groupID, [CallerMemberName] string caller = null)
            {
                DBC.ECS.Check.Require(groupID.isInvalid == false, "invalid group detected");
                _enginesRoot.Target.RemoveGroupID(groupID);

                _enginesRoot.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, new EGID(0, groupID), new EGID(), null, caller));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntitiesInGroup<T>(ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new()
            {
                if (_enginesRoot.Target._groupEntityComponentsDB.TryGetValue(
                        fromGroupID.group, out FasterDictionary<RefWrapperType, ITypeSafeDictionary> entitiesInGroupPerType)
                 == true)
                {
#if DEBUG && !PROFILE_SVELTO
                    IComponentBuilder[] components = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;
                    var dictionary = entitiesInGroupPerType[new RefWrapperType(components[0].GetEntityComponentType())];

                    dictionary.KeysEvaluator((key) =>
                    {
                        _enginesRoot.Target.CheckRemoveEntityID(new EGID(key, fromGroupID), TypeCache<T>.type, caller);
                        _enginesRoot.Target.CheckAddEntityID(new EGID(key, toGroupID), TypeCache<T>.type, caller);
                    });

#endif
                    _enginesRoot.Target.QueueEntitySubmitOperation(
                        new EntitySubmitOperation(EntitySubmitOperationType.SwapGroup, new EGID(0, fromGroupID)
                                                , new EGID(0, toGroupID), null, caller));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(uint entityID, ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(new EGID(entityID, fromGroupID), toGroupID, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null)            
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(fromID, new EGID(fromID.entityID, toGroupID), caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, ExclusiveBuildGroup mustBeFromGroup, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null)              
                where T : IEntityDescriptor, new()
            {
                if (fromID.groupID != mustBeFromGroup)
                    throw new ECSException($"Entity is not coming from the expected group. Expected {mustBeFromGroup} is {fromID.groupID}");

                SwapEntityGroup<T>(fromID, toGroupID, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, EGID toID, ExclusiveBuildGroup mustBeFromGroup, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new()
            {
                if (fromID.groupID != mustBeFromGroup)
                    throw new ECSException($"Entity is not coming from the expected group Expected {mustBeFromGroup} is {fromID.groupID}");

                SwapEntityGroup<T>(fromID, toID);
            }

#if UNITY_NATIVE
            public Native.NativeEntityRemove ToNativeRemove<T>(string memberName) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.ProvideNativeEntityRemoveQueue<T>(memberName);
            }

            public Native.NativeEntitySwap ToNativeSwap<T>(string memberName) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.ProvideNativeEntitySwapQueue<T>(memberName);
            }
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, EGID toID, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new()
            {
                DBC.ECS.Check.Require(fromID.groupID.isInvalid == false, "invalid group detected");
                DBC.ECS.Check.Require(toID.groupID.isInvalid == false, "invalid group detected");

                var enginesRootTarget           = _enginesRoot.Target;
                var descriptorComponentsToBuild = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;
                
                enginesRootTarget.CheckRemoveEntityID(fromID, TypeCache<T>.type, caller);
                enginesRootTarget.CheckAddEntityID(toID, TypeCache<T>.type, caller);

                enginesRootTarget.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap,
                        fromID, toID, descriptorComponentsToBuild, caller));
            }

            //enginesRoot is a weakreference because GenericEntityStreamConsumerFactory can be injected inside
            //engines of other enginesRoot
            readonly WeakReference<EnginesRoot> _enginesRoot;
        }

        void QueueEntitySubmitOperation(EntitySubmitOperation entitySubmitOperation)
        {
            _entitiesOperations.Add((ulong) entitySubmitOperation.fromID, entitySubmitOperation);
        }

        void QueueEntitySubmitOperation<T>(EntitySubmitOperation entitySubmitOperation) where T : IEntityDescriptor
        {
#if DEBUG && !PROFILE_SVELTO
            if (_entitiesOperations.TryGetValue((ulong) entitySubmitOperation.fromID, out var entitySubmitedOperation))
            {
                if (entitySubmitedOperation != entitySubmitOperation)
                    throw new ECSException("Only one entity operation per submission is allowed"
                       .FastConcat(" entityComponentType: ")
                       .FastConcat(typeof(T).Name)
                       .FastConcat(" submission type ", entitySubmitOperation.type.ToString(),
                            " from ID: ", entitySubmitOperation.fromID.entityID.ToString())
                       .FastConcat(" previous operation type: ",
                            _entitiesOperations[(ulong) entitySubmitOperation.fromID].type
                               .ToString()));
            }
            else
#endif
                _entitiesOperations[(ulong) entitySubmitOperation.fromID] = entitySubmitOperation;
        }
    }
}