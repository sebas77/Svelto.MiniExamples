#if UNITY_ECS
using Svelto.Common;
using Svelto.DataStructures;
using Unity.Collections;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        struct NativeOperationRemove
        {
            internal readonly IEntityBuilder[]  entityComponents;
            internal readonly NativeQueue<EGID> queue;

            public NativeOperationRemove(NativeQueue<EGID> simpleQueue, IEntityBuilder[] descriptorEntitiesToBuild)
            {
                entityComponents = descriptorEntitiesToBuild;
                queue            = simpleQueue;
            }
        }
        
        struct NativeOperationSwap
        {
            internal readonly IEntityBuilder[]  entityComponents;
            internal readonly NativeQueue<DoubleEGID> queue;

            public NativeOperationSwap(NativeQueue<DoubleEGID> simpleQueue, IEntityBuilder[] descriptorEntitiesToBuild)
            {
                entityComponents = descriptorEntitiesToBuild;
                queue            = simpleQueue;
            }
        }

        NativeEntityOperations ProvideNativeEntityRemoveQueue<T>(Allocator allocator) where T : IEntityDescriptor, new()
        {
            NativeQueue<EGID> egidsToRemove = new NativeQueue<EGID>(allocator);

            _nativeRemoveOperations.Add(new NativeOperationRemove(egidsToRemove
                                                                , EntityDescriptorTemplate<T>
                                                                 .descriptor.entityComponentsToBuild));
            NativeQueue<DoubleEGID> egidsToSwap = new NativeQueue<DoubleEGID>(allocator);
            _nativeSwapOperations.Add(
                new NativeOperationSwap(egidsToSwap, EntityDescriptorTemplate<T>.descriptor.entityComponentsToBuild));

            return new NativeEntityOperations(egidsToRemove, egidsToSwap);
        }

        void NativeOperationSubmission(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Native Remove Operations"))
            {
                for (int i = 0; i < _nativeRemoveOperations.count; i++)
                {
                    var simpleNativeArray = _nativeRemoveOperations[i].queue;

                    while (simpleNativeArray.TryDequeue(out EGID entityEGID))
                    {
                        CheckRemoveEntityID(entityEGID);
                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Remove, entityEGID, entityEGID
                                                     , _nativeRemoveOperations[i].entityComponents));
                    }
                }
            }
        }

        void AllocateNativeOperations()
        {
            _nativeRemoveOperations = new FasterList<NativeOperationRemove>();
            _nativeSwapOperations   = new FasterList<NativeOperationSwap>();
        }

        void DisposeNativeOperations(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Native Dispose Operations"))
            {
                for (int i = 0; i < _nativeRemoveOperations.count; i++)
                    _nativeRemoveOperations[i].queue.Dispose();

                for (int i = 0; i < _nativeSwapOperations.count; i++)
                    _nativeSwapOperations[i].queue.Dispose();

                _nativeRemoveOperations.FastClear();
                _nativeSwapOperations.FastClear();
            }
        }

        FasterList<NativeOperationRemove> _nativeRemoveOperations;
        FasterList<NativeOperationSwap> _nativeSwapOperations;
    }

    struct DoubleEGID
    {
        internal readonly EGID from;
        internal readonly EGID to;
        
        public DoubleEGID(EGID from1, EGID to1)
        {
            from = from1;
            to = to1;
        }
    }

    public struct NativeEntityOperations
    {
        NativeQueue<EGID>.ParallelWriter EGIDsToRemove;
        NativeQueue<DoubleEGID>.ParallelWriter EGIDsToSwap;

        internal NativeEntityOperations(NativeQueue<EGID> EGIDsToRemove, NativeQueue<DoubleEGID> EGIDsToSwap)
        {
            this.EGIDsToRemove = EGIDsToRemove.AsParallelWriter();
            this.EGIDsToSwap   = EGIDsToSwap.AsParallelWriter();
        }

        public void RemoveEntity(EGID egid) { EGIDsToRemove.Enqueue(egid); }
        public void SwapEntity(EGID from, EGID to) { EGIDsToSwap.Enqueue(new DoubleEGID(from, to)); }
    }
}
#endif