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
    public class SveltoCompositionRoot: ICompositionRoot
    {
        public void OnContextCreated<T>(T contextHolder)
        {
            QualitySettings.vSyncCount = -1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _simpleSubmitScheduler = new SimpleEntitiesSubmissionScheduler();
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
            _sveltoOverDotsEnginesGroupEnginesGroup.Dispose();
            _enginesRoot.Dispose();
            _mainLoop?.Dispose();
            _simpleSubmitScheduler.Dispose();
        }

        async Task ComposeEnginesRoot()
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();
            var entityFunctions = _enginesRoot.GenerateEntityFunctions();

            //SveltoOnDOTSEnginesGroup is the DOTS Wrapper to facilitate Svelto <-> DOTS integration
            _sveltoOverDotsEnginesGroupEnginesGroup = new SveltoOnDOTSEnginesGroup(_enginesRoot);
            _enginesToTick.Add(_sveltoOverDotsEnginesGroupEnginesGroup);

            var (redFoodPrefab, blueFoodPrefab, redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab) =
                    await LoadAssetAndCreatePrefabs(_sveltoOverDotsEnginesGroupEnginesGroup.world);

            EntityManager worldEntityManager = _sveltoOverDotsEnginesGroupEnginesGroup.world.EntityManager;
            worldEntityManager.AddComponent<SpecialBluePrefab>(specialBlueDoofusPrefab);

            //Standard Svelto Engines
            AddSveltoEngineToTick(new SpawningDoofusEngine(redDoofusPrefab, blueDoofusPrefab, specialBlueDoofusPrefab, entityFactory));
            AddSveltoEngineToTick(new SpawnFoodOnClickEngine(redFoodPrefab, blueFoodPrefab, entityFactory));
            AddSveltoEngineToTick(new ConsumingFoodEngine(entityFunctions));
            AddSveltoEngineToTick(new LookingForFoodDoofusesEngine(entityFunctions));
            AddSveltoEngineToTick(new VelocityToPositionDoofusesEngine());

            //Svelto engines that runs 
            _sveltoOverDotsEnginesGroupEnginesGroup.AddSveltoToDOTSEngine(new RenderingDOTSDataSynchronizationEngine());
            _sveltoOverDotsEnginesGroupEnginesGroup.AddSveltoOnDOTSSubmissionEngine(new SpawnUnityEntityOnSveltoEntityEngine());

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

            return (component.RedFood, component.BlueFood, component.RedDoofus, component.BlueDoofus, component.SpecialDoofus);
        }

        void AddSveltoEngineToTick(IJobifiedEngine engine)
        {
            _enginesRoot.AddEngine(engine);
            
            _enginesToTick.Add(engine);
        }
        
        EnginesRoot _enginesRoot;
        readonly FasterList<IJobifiedEngine> _enginesToTick = new FasterList<IJobifiedEngine>();
        SimpleEntitiesSubmissionScheduler _simpleSubmitScheduler;
        SveltoOnDOTSEnginesGroup _sveltoOverDotsEnginesGroupEnginesGroup;
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