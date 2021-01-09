using System;
using System.Collections;
using System.Threading;
using Svelto.Common;
using Svelto.ECS.EntityComponents;
using Svelto.ECS.Extensions.Unity;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;
using Svelto.Tasks.ExtraLean;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.PlaceFoodOnClickEngine))]
    public class PlaceFoodOnClickEngine : IQueryingEntitiesEngine, IJobifiedEngine
    {
        const int MaxMeals = 500;

        public PlaceFoodOnClickEngine(Entity redfood, Entity bluefood, IEntityFactory entityFactory)
        {
            //read why I am using a Native Factory here in the Execute method
            _entityFactory = entityFactory.ToNative<FoodEntityDescriptor>("PlaceFoodOnClickEngine");
            _redfood       = redfood;
            _bluefood      = bluefood;
        }

        public string name => nameof(PlaceFoodOnClickEngine);

        IEnumerator CheckClick()
        {
            var timer = new ReusableWaitForSecondsEnumerator(0.01f);

            while (true)
            {
                //note: in a complex project an engine shouldn't ever poll input directly, it should instead poll
                //entity states
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1) == true)
                {
                    var _random = new Random((uint) DateTime.Now.Ticks);
                    //I am cheating a bit with the MouseToPosition function, but for the purposes of this demo
                    //creating a Camera Entity was an overkill
                    if (UnityUtilities.MouseToPosition(out Vector3 position))
                    {
                        //BuildEntity returns an EntityInitialized that is used to set the default values of the
                        //entity that will be built.
                        for (int i = 0; i < MaxMeals; i++)
                        {
                            NativeEntityInitializer init; 

                            var randX       = position.x + _random.NextFloat(-50, 50);
                            var randZ       = position.z + _random.NextFloat(-50, 50);
                            var newposition = new float3(randX, position.y, randZ);

                            bool isRed;

                            if (Input.GetMouseButton(0))
                            {
                                init = _entityFactory.BuildEntity(
                                    new EGID(_foodPlaced++, GameGroups.RED_FOOD_NOT_EATEN.BuildGroup)
                                  , Thread.CurrentThread.ManagedThreadId);
                                isRed = true;
                            }
                            else
                            {
                                init = _entityFactory.BuildEntity(
                                    new EGID(_foodPlaced++, GameGroups.BLUE_FOOD_NOT_EATEN.BuildGroup)
                                  , Thread.CurrentThread.ManagedThreadId);

                                isRed = false;
                            }

                            init.Init(new PositionEntityComponent
                            {
                                position = newposition
                            });
                            //these structs are used for ReactOnAdd callback to create unity Entities later
                            init.Init(new UnityEcsEntityComponent
                            {
                                uecsEntity    = isRed ? _redfood : _bluefood
                              , spawnPosition = newposition
                               ,
                            });
                        }

                        while (timer.IsDone() == false)
                            yield return Yield.It;
                    }
                }

                yield return Yield.It;
            }
        }

        public EntitiesDB entitiesDB { private get; set; }

        public void Ready() { CheckClick().RunOn(UIInteractionRunner); }
        
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
        public JobHandle Execute(JobHandle _jobHandle)
        {
            UIInteractionRunner.Step();

            return _jobHandle;
        }

        static readonly SteppableRunner UIInteractionRunner = new SteppableRunner("UIInteraction");

        readonly NativeEntityFactory _entityFactory;
        readonly Entity         _redfood;
        readonly Entity         _bluefood;

        uint _foodPlaced;
    }
}