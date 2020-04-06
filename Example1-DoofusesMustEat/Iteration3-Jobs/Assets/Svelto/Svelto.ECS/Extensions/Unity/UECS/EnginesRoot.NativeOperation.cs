#if UNITY_ECS
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Svelto.ECS.DataStructures.Unity;
using Unity.Jobs.LowLevel.Unsafe;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        //todo: I very likely don't need to create one for each native entity factory, the same can be reused
        readonly MultiAppendBuffer _addOperationQueue =
            new MultiAppendBuffer(Common.Allocator.Persistent, JobsUtility.MaxJobThreadCount + 1);

        readonly MultiAppendBuffer _removeOperationQueue =
            new MultiAppendBuffer(Common.Allocator.Persistent, JobsUtility.MaxJobThreadCount + 1);

        readonly MultiAppendBuffer _swapOperationQueue =
            new MultiAppendBuffer(Common.Allocator.Persistent, JobsUtility.MaxJobThreadCount + 1);

        NativeEntityRemove ProvideNativeEntityRemoveQueue<T>() where T : IEntityDescriptor, new()
        {
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeRemoveOperations.Add(
                new NativeOperationRemove(EntityDescriptorTemplate<T>.descriptor.componentsToBuild));

            return new NativeEntityRemove(_removeOperationQueue, _nativeRemoveOperations.count - 1);
        }
        
        NativeEntitySwap ProvideNativeEntitySwapQueue<T>() where T : IEntityDescriptor, new()
        {
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeSwapOperations.Add(
                new NativeOperationSwap(EntityDescriptorTemplate<T>.descriptor.componentsToBuild));

            return new NativeEntitySwap(_swapOperationQueue, _nativeSwapOperations.count - 1);
        }

        NativeEntityFactory ProvideNativeEntityFactoryQueue<T>() where T : IEntityDescriptor, new()
        {
            //todo: remove operation array and store entity descriptor hash in the return value
            _nativeAddOperations.Add(
                new NativeOperationBuild(EntityDescriptorTemplate<T>.descriptor.componentsToBuild));

            return new NativeEntityFactory(_addOperationQueue, _nativeAddOperations.count - 1);
        }

        void NativeOperationSubmission(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Native Remove/Swap Operations"))
            {
                for (int i = 0; i < _removeOperationQueue.count; i++)
                {
                    ref var buffer = ref _removeOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        Svelto.Console.Log("dequeue " + componentsIndex);
                        var entityEGID = buffer.Dequeue<EGID>();
                        Svelto.Console.Log("dequeue " + entityEGID);
                        CheckRemoveEntityID(entityEGID);
                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Remove, entityEGID, entityEGID
                                                     , _nativeRemoveOperations[componentsIndex].entityComponents));
                    }
                }

                for (int i = 0; i < _swapOperationQueue.count; i++)
                {
                    ref var buffer = ref _swapOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var     componentsIndex = buffer.Dequeue<uint>();
                        var entityEGID      = buffer.Dequeue<DoubleEGID>();
                        
                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Swap, entityEGID.@from, entityEGID.to
                                                     , _nativeSwapOperations[componentsIndex].entityComponents));
                    }
                }
            }

            using (profiler.Sample("Native Add Operations"))
            {
                for (int i = 0; i < _addOperationQueue.count; i++)
                {
                    ref var buffer = ref _addOperationQueue.GetBuffer(i);

                    while (buffer.IsEmpty() == false)
                    {
                        var componentsIndex = buffer.Dequeue<uint>();
                        var egid            = buffer.Dequeue<EGID>();
                        var componentCounts = buffer.Dequeue<uint>();
                        EntityComponentInitializer init =
                            BuildEntity(egid, _nativeAddOperations[componentsIndex].components);

                        while (componentCounts > 0)
                        {
                            componentCounts--;

                            var typeID = buffer.Dequeue<uint>();

                            IFiller entityBuilder = EntityComponentIDMap.GetTypeFromID(typeID);

                            //after the typeID, I expect the serialized component
                            entityBuilder.FillFromByteArray(init, buffer);
                        }
                    }
                }
            }
        }

        void AllocateNativeOperations()
        {
            _nativeRemoveOperations = new FasterList<NativeOperationRemove>();
            _nativeSwapOperations   = new FasterList<NativeOperationSwap>();
            _nativeAddOperations    = new FasterList<NativeOperationBuild>();
        }

        FasterList<NativeOperationRemove> _nativeRemoveOperations;
        FasterList<NativeOperationSwap>   _nativeSwapOperations;
        FasterList<NativeOperationBuild>  _nativeAddOperations;
    }

    readonly struct DoubleEGID
    {
        internal readonly EGID from;
        internal readonly EGID to;

        public DoubleEGID(EGID from1, EGID to1)
        {
            from = from1;
            to   = to1;
        }
    }

    public readonly struct NativeEntityRemove
    {
        readonly MultiAppendBuffer _removeQueue;
        readonly uint              _indexRemove;

        internal NativeEntityRemove(MultiAppendBuffer EGIDsToRemove, uint indexRemove)
        {
            _removeQueue = EGIDsToRemove;
            _indexRemove = indexRemove;
        }

        public void RemoveEntity(EGID egid, int threadIndex)
        {
            var simpleNativeBag = _removeQueue.GetBuffer(threadIndex);
            Svelto.Console.Log("<color=red>remove index</color> " + _indexRemove);
            simpleNativeBag.Enqueue(_indexRemove);
            Svelto.Console.Log("<color=yellow>remove EGID</color> " + egid.ToString());
            simpleNativeBag.Enqueue(egid);
        }
    }
    
    public readonly struct NativeEntitySwap
    {
        readonly MultiAppendBuffer _swapQueue;
        readonly uint              _indexSwap;

        internal NativeEntitySwap(MultiAppendBuffer EGIDsToSwap, uint indexSwap)
        {
            _swapQueue   = EGIDsToSwap;
            _indexSwap   = indexSwap;
        }

        public void SwapEntity(EGID from, EGID to, int threadIndex)
        {
            var simpleNativeBag = _swapQueue.GetBuffer(threadIndex);
            simpleNativeBag.Enqueue(_indexSwap);
            simpleNativeBag.Enqueue(new DoubleEGID(from, to));
        }

        public void SwapEntity(EGID from, ExclusiveGroupStruct to, int threadIndex)
        {
            var simpleNativeBag = _swapQueue.GetBuffer(threadIndex);
            simpleNativeBag.Enqueue(_indexSwap);
            simpleNativeBag.Enqueue(new DoubleEGID(from, new EGID(from.entityID, to)));
        }
    }

    public readonly struct NativeEntityFactory
    {
        readonly MultiAppendBuffer _addOperationQueue;
        readonly uint              _index;

        internal NativeEntityFactory(MultiAppendBuffer addOperationQueue, uint index)
        {
            _index             = index;
            _addOperationQueue = addOperationQueue;
        }

        public NativeEntityComponentInitializer BuildEntity
            (uint eindex, ExclusiveGroupStruct buildGroup, int threadIndex)
        {
            NativeRingBuffer unsafeBuffer = _addOperationQueue.GetBuffer(threadIndex + 1);

            unsafeBuffer.Enqueue(_index);
            unsafeBuffer.Enqueue(new EGID(eindex, buildGroup));
            unsafeBuffer.ReserveEnqueue<uint>(out var index) = 0;

            return new NativeEntityComponentInitializer(unsafeBuffer, index);
        }
    }

    public readonly ref struct NativeEntityComponentInitializer
    {
        readonly NativeRingBuffer  _unsafeBuffer;
        readonly UnsafeArrayIndex _index;

        public NativeEntityComponentInitializer(in NativeRingBuffer unsafeBuffer, UnsafeArrayIndex index)
        {
            _unsafeBuffer = unsafeBuffer;
            _index        = index;
        }

        public void Init<T>(in T component) where T : unmanaged, IEntityComponent
        {
            uint id = EntityComponentIDMap.GetIDFromType<T>();

            _unsafeBuffer.AccessReserved<uint>(_index)++;

            _unsafeBuffer.Enqueue(id);
            _unsafeBuffer.Enqueue(component);
        }
    }

    struct NativeOperationBuild
    {
        internal readonly IEntityComponentBuilder[] components;

        public NativeOperationBuild(IEntityComponentBuilder[] descriptorEntityComponentsToBuild)
        {
            components = descriptorEntityComponentsToBuild;
        }
    }

    readonly struct NativeOperationRemove
    {
        internal readonly IEntityComponentBuilder[] entityComponents;

        public NativeOperationRemove(IEntityComponentBuilder[] descriptorEntitiesToBuild)
        {
            entityComponents = descriptorEntitiesToBuild;
        }
    }

    readonly struct NativeOperationSwap
    {
        internal readonly IEntityComponentBuilder[] entityComponents;

        public NativeOperationSwap(IEntityComponentBuilder[] descriptorEntitiesToBuild)
        {
            entityComponents = descriptorEntitiesToBuild;
        }
    }
}
#endif