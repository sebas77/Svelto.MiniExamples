using System.Threading.Tasks;
using Svelto.Context;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Scenes;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    /// <summary>
    /// This is a standard Svelto Composition Root, the DOTS bit will come later
    /// </summary>
    public class SveltoCompositionRoot: ICompositionRoot
    {
        public void OnContextCreated<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            //create the standard sheduler
            _simpleSubmitScheduler = new SimpleEntitiesSubmissionScheduler();
            //create the standard engines root
            _enginesRoot = new EnginesRoot(_simpleSubmitScheduler);
        }

        public void OnContextInitialized<T>(T contextHolder)
        {
            JobsUtility.JobDebuggerEnabled = true;
            
#pragma warning disable CS4014
            ComposeEnginesRoot();
#pragma warning restore CS4014
        }

        public void OnContextDestroyed(bool isInitialized)
        {
            _sveltoOverDotsEnginesGroup.Dispose();
            _enginesRoot.Dispose();
            _mainLoop?.Dispose();
            _simpleSubmitScheduler.Dispose();
        }

        async Task ComposeEnginesRoot()
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            //The core of SveltoOnDOTS. SveltoOnDOTSEnginesGroup is the DOTS Wrapper to facilitate Svelto <-> DOTS integration
            //fundamentally is a Svelto engine container about all the SveltoOnDOTS engines. these engines are:
            //1) ISveltoOnDOTSStructuralEngine: these engines are used to create DOTS entities over Svelto entities
            //2) SyncSveltoToDOTSEngine: these engines DOTS SystemBase engines and are used to sync Svelto data to DOTS data
            //3) SyncDOTSToSveltoEngine: these engines DOTS SystemBase engines and are used to sync DOTS data to Svelto data
            //4) pure DOTS SystemBase/ISystem engines: these engines are used to perform DOTS only logic
            
            _sveltoOverDotsEnginesGroup = new SveltoOnDOTSEnginesGroup(_enginesRoot);
            _enginesToTick.Add(_sveltoOverDotsEnginesGroup);

            var (redFoodPrefab, blueFoodPrefab, redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab) =
                    await LoadAssetAndCreatePrefabs(_sveltoOverDotsEnginesGroup.world);

            //Standard Svelto Engines
            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());
            
            //SveltoOnDots structural engines these are added through
            //sveltoOverDotsEnginesGroupEnginesGroup.AddSveltoOnDOTSSubmissionEngine(structuralEngine);
            AddSveltoEngineToTick(new SpawningDoofusEngine(redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab, entityFactory));
            AddSveltoEngineToTick(new SpawnFoodOnClickEngine(redFoodPrefab, blueFoodPrefab, entityFactory));

            //SveltoOnDOTS sync engines these are added through
            //sveltoOverDotsEnginesGroupEnginesGroup.AddSveltoToDOTSSyncEngine(syncEngine); or
            //sveltoOverDotsEnginesGroupEnginesGroup.AddDOTSToSveltoSyncEngine(syncEngine);
            //depending on the direction of the sync
            _sveltoOverDotsEnginesGroup.AddSveltoToDOTSSyncEngine(new RenderingDOTSPositionSyncEngine());
            
            //being _sveltoOverDotsEnginesGroup an engine group, it will tick all the engines added to it (structural and sync)
            //it will also run the DOTS ECS systems linked to the DOTS World created by the SveltoOnDOTSEnginesGroup
            //it is expected to be ticked too, so normally it's found inside a SortedEnginesGroup
            //A standard SveltoOOnDOTS frame runs like
            // Svelto (GameLogic) Engines Run first (thanks to the SortedEnginesGroup, it's basically user choice)
            // Then this Engine is ticked, causing:
            // All the Jobs to be completed (it's a sync point)
            // Synchronizations engines to be executed (Svelto to DOTS ECS)
            // Submission of Entities to be executed
            // Svelto Add/Remove callbacks to be called
            // ISveltoOnDOTSStructuralEngine to be executed
            /// DOTS ECS engines to executed
            /// Synchronizations engines to be executed (DOTS ECS To Svelto)
            _mainLoop = new MainLoop(_enginesToTick);
            _mainLoop.Run();
        }

        static async Task<(Entity, Entity, Entity, Entity, Entity)> LoadAssetAndCreatePrefabs(World customWorld)
        {
            var customWorldUnmanaged = customWorld.Unmanaged;
            var sceneGuid = new Hash128("52b77c03e0aae864286b8f57b3216c18");
            var sceneEntity = SceneSystem.LoadSceneAsync(customWorldUnmanaged, new EntitySceneReference(sceneGuid, 0));

            while (SceneSystem.IsSceneLoaded(customWorldUnmanaged, sceneEntity) == false)
            {
                customWorld.Update();
                await Task.Yield();
            }

            EntityManager worldEntityManager = customWorld.EntityManager;
            var query = worldEntityManager.CreateEntityQuery(typeof(PrefabsComponents));
            var prefabHolder = query.ToComponentDataArray<PrefabsComponents>(Allocator.Temp);

            var component = prefabHolder[0];
            //add the DOTS special blue IEnableable component so that we can filter them later for the sync of the positions
            worldEntityManager.AddComponent<SpecialBluePrefab>(component.SpecialDoofus);

            return (component.RedFood, component.BlueFood, component.RedDoofus, component.BlueDoofus, component.SpecialDoofus);
        }

        void AddSveltoEngineToTick(IJobifiedEngine engine)
        {
            _enginesToTick.Add(engine);
            
            if (engine is ISveltoOnDOTSStructuralEngine structuralEngine)
                _sveltoOverDotsEnginesGroup.AddSveltoOnDOTSSubmissionEngine(structuralEngine);
            else
                _enginesRoot.AddEngine(engine);
        }
        
        EnginesRoot _enginesRoot;
        readonly FasterList<IJobifiedEngine> _enginesToTick = new FasterList<IJobifiedEngine>();
        SimpleEntitiesSubmissionScheduler _simpleSubmitScheduler;
        SveltoOnDOTSEnginesGroup _sveltoOverDotsEnginesGroup;
        MainLoop _mainLoop;
    }
}

//[BurstCompile]
//    unsafe struct GatherEntitiesJob : IJobChunk
//    {
//        [NativeDisableParallelForRestriction] public TypelessUnsafeList OutputList;
//        [ReadOnly] public EntityTypeHandle EntityTypeHandle;
//        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<int> ChunkBaseEntityIndices;
//
//        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
//        {
//            int baseEntityIndexInQuery= ChunkBaseEntityIndices[unfilteredChunkIndex];
//            Entity* dstEntities = (Entity*)OutputList.Ptr + baseEntityIndexInQuery;
//            Entity* srcEntities = chunk.GetEntityDataPtrRO(EntityTypeHandle);
//            int chunkEntityCount = chunk.Count;
//            int copyCount = useEnabledMask ? EnabledBitUtility.countbits(chunkEnabledMask) : chunkEntityCount;
//#if ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG
//            Entity* dstEnd = (Entity*)OutputList.Ptr + OutputList.Capacity;
//            if (dstEntities + copyCount > dstEnd)
//            {
//                // ERROR: this means there were more entities to copy than we were expecting, and we're about to
//                // write off the end of the output list.
//                throw new InvalidOperationException($"Internal error: detected buffer overrun in {nameof(GatherEntitiesJob)}");
//            }
//#endif
//            if (useEnabledMask)
//            {
//                v128 maskCopy = chunkEnabledMask;
//                int rangeStart = 0;
//                int rangeEnd = 0;
//                int numCopied = 0;
//                while (EnabledBitUtility.GetNextRange(ref maskCopy, ref rangeStart, ref rangeEnd))
//                {
//                    int rangeCount = rangeEnd - rangeStart;
//                    UnsafeUtility.MemCpy(dstEntities+numCopied, srcEntities+rangeStart, rangeCount * sizeof(Entity));
//                    numCopied += rangeCount;
//                }
//            }
//            else
//            {
//                UnsafeUtility.MemCpy(dstEntities, srcEntities, chunk.Count * sizeof(Entity));
//            }
//            Interlocked.Add(ref *(OutputList.Length), copyCount);
//        }

//#include <iostream>
//#include <vector>
//#include <bitset>
//#include <immintrin.h>
//
//int main() {
//    // Create an array of 16 integers for demonstration purposes
//    std::vector<int> arr(16);
//    for (int i = 0; i < arr.size(); i++) {
//        arr[i] = i;
//    }
//
//    // Create a bitset representing the subset of even indices
//    std::bitset<16> subset;
//    for (int i = 0; i < subset.size(); i += 2) {
//        subset.set(i);
//    }
//
//    // Vectorize the subset iteration using SSE instructions
//    __m128i subset_vec = _mm_set_epi16(subset.to_ulong(), 0, 0, 0, 0, 0, 0, 0);
//    for (int i = 0; i < arr.size(); i += 8) {
//        __m128i arr_vec = _mm_loadu_si128((__m128i*)(arr.data() + i));
//        __m128i result = _mm_and_si128(subset_vec, arr_vec);
//        int subset_arr[8];
//        _mm_storeu_si128((__m128i*)subset_arr, result);
//        for (int j = 0; j < 8; j++) {
//            if (subset_arr[j]) {
//                int elem = arr[i + j];
//                // do something with the element
//                std::cout << elem << " ";
//            }
//        }
//    }
//    // Output: 0 2 4 6 8 10 12 14
//    return 0;
//}