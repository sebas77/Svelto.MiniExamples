using System;
using System.Collections;
using System.Threading;
using Svelto.Common;
using Svelto.ECS.Miniexamples.Doofuses.GameObjectsLayer;
using Svelto.ECS.Native;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Svelto.ECS.Miniexamples.Doofuses.Gameobjects
{
    [Sequenced(nameof(DoofusesEngineNames.PlaceFoodOnClickEngine))]
    public class PlaceFoodOnClickEngine : IQueryingEntitiesEngine, IJobifiedEngine
    {
        const int MaxMeals = 250;

        public PlaceFoodOnClickEngine(int redfood, int bluefood, IEntityFactory entityFactory)
        {
            //read why I am using a Native Factory here in the Execute method
            _entityFactory = entityFactory.ToNative<FoodEntityDescriptor>("PlaceFoodOnClickEngine");
            _redfood       = redfood;
            _bluefood      = bluefood;
        }

        public string name => nameof(PlaceFoodOnClickEngine);

        IEnumerator CheckClick()
        {
            var threadLocalRandom = new ThreadLocalRandom(256);
            
            while (true)
            {
                //note: in a complex project an engine shouldn't ever poll input directly, it should instead poll
                //entity states
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1) == true)
                {
                    //I am cheating a bit with the MouseToPosition function, but for the purposes of this demo
                    //creating a Camera Entity was an overkill
                    if (UnityUtilities.MouseToPosition(out Vector3 position))
                    {
                        if (Input.GetMouseButton(0))
                        {
                            _inputDeps =
                                new PlaceFood(position, _entityFactory, GameGroups.RED_FOOD_NOT_EATEN.BuildGroup
                                  , _redfood, _foodPlaced, threadLocalRandom).ScheduleParallel(MaxMeals, _inputDeps);
                        }
                        else
                        {
                            _inputDeps =
                                new PlaceFood(position, _entityFactory, GameGroups.BLUE_FOOD_NOT_EATEN.BuildGroup
                                  , _bluefood, _foodPlaced, threadLocalRandom).ScheduleParallel(MaxMeals, _inputDeps);
                        }

                        _foodPlaced += MaxMeals;

                        var now = DateTime.Now;
                        while ((DateTime.Now - now).TotalMilliseconds < 100)
                            yield return null;
                    }
                }

                yield return null;
            }
        }

        [BurstCompile]
        struct PlaceFood : IJobParallelFor
        {
            readonly Vector3                    _position;
            readonly NativeEntityFactory        _entityFactory;
            readonly ExclusiveBuildGroup        _exclusiveBuildGroup;
            readonly int                        _prefabID;
            readonly uint                       _foodPlaced;
            readonly ThreadLocalRandom          _threadLocalRandom;
            [NativeSetThreadIndex] readonly int _threadIndex;

            public PlaceFood
            (Vector3 position, NativeEntityFactory factory, ExclusiveBuildGroup exclusiveBuildGroup, int prefabID
           , uint foodPlaced, ThreadLocalRandom threadLocalRandom) : this()
            {
                _position            = position;
                _entityFactory       = factory;
                _exclusiveBuildGroup = exclusiveBuildGroup;
                _prefabID            = prefabID;
                _foodPlaced          = foodPlaced;
                _threadLocalRandom   = threadLocalRandom;
            }

            public void Execute(int index)
            {
                var randX       = _position.x + _threadLocalRandom.Next(-50, 50, _threadIndex);
                var randZ       = _position.z + _threadLocalRandom.Next(-50, 50, _threadIndex);
                var newposition = new float3(randX, _position.y, randZ);

                //BuildEntity returns an EntityInitialized that is used to set the default values of the
                //entity that will be built.
                var init = _entityFactory.BuildEntity(new EGID((uint) (_foodPlaced + index), _exclusiveBuildGroup)
                                                    , _threadIndex);

                init.Init(new PositionEntityComponent
                {
                    position = newposition
                });
                //these structs are used for ReactOnAdd callback to create unity Entities later
                init.Init(new GameObjectEntityComponent
                {
                    prefabID      = _prefabID
                  , spawnPosition = newposition
                   ,
                });
            }
        }

        public EntitiesDB entitiesDB { private get; set; }

        public void Ready()
        {
            _taskRunner = CheckClick();
        }

        /// <summary>
        /// Beware of this engine. Reading directly the input like I am doing in this class is a bad practice
        /// The input should always be read in separate input specialised classes and translated in pure ECS values,
        /// for example in actions. For Jobified engines this is even more important.
        /// It is not possible to mix jobified code with not jobified code without taking care of the dependencies.
        /// Completing a job passed as dependency is also a very bad practice.
        /// However if the job is not completed, race conditions are very likely to occur. Svelto.ECS is not thread safe
        /// and in this specific case building entities would cause a race condition of entities are built
        /// in jobified engines too.
        /// In this case I solved the problem using the NativeEntityFactory which is by design thread safe.
        /// </summary>
        /// <param name="inputDeps"></param>
        /// <returns></returns>
        public JobHandle Execute(JobHandle inputDeps)
        {
            _inputDeps = inputDeps;

            _taskRunner.MoveNext();

            return _inputDeps;
        }

        readonly int _redfood;
        readonly int _bluefood;
        uint         _foodPlaced;

        readonly NativeEntityFactory _entityFactory;
        JobHandle                    _inputDeps;
        static IEnumerator           _taskRunner;
    }
}