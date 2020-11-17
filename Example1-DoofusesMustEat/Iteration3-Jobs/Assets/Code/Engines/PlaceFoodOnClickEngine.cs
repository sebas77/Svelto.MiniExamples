using System;
using System.Collections;
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
            _entityFactory = entityFactory;
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
                            EntityComponentInitializer init; 

                            var randX       = position.x + _random.NextFloat(-50, 50);
                            var randZ       = position.z + _random.NextFloat(-50, 50);
                            var newposition = new float3(randX, position.y, randZ);

                            bool isRed;

                            if (Input.GetMouseButton(0))
                            {
                                init = _entityFactory.BuildEntity<FoodEntityDescriptor>(
                                    _foodPlaced++, GameGroups.RED_FOOD_NOT_EATEN.BuildGroup);

                                isRed = true;
                            }
                            else
                            {
                                init = _entityFactory.BuildEntity<FoodEntityDescriptor>(
                                    _foodPlaced++, GameGroups.BLUE_FOOD_NOT_EATEN.BuildGroup);

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

        static readonly SteppableRunner UIInteractionRunner = new SteppableRunner("UIInteraction");

        readonly IEntityFactory _entityFactory;
        readonly Entity         _redfood;
        readonly Entity         _bluefood;

        uint _foodPlaced;

        public JobHandle Execute(JobHandle _jobHandle)
        {
            UIInteractionRunner.Step();

            return _jobHandle;
        }
    }
}