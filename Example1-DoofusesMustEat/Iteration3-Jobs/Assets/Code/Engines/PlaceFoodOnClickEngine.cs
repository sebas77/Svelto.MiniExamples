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
    public class PlaceFoodOnClickEngine : IQueryingEntitiesEngine, IJobifiableEngine
    {
        public PlaceFoodOnClickEngine(Entity redfood, Entity bluefood, IEntityFactory entityFactory)
        {
            _entityFactory = entityFactory;
            _redfood          = redfood;
            _bluefood = bluefood;
        }

        IEnumerator CheckClick()
        {
            var timer = new ReusableWaitForSecondsEnumerator(0.01f);

            while (true)
            {
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1) == true)
                {
                    var _random = new Random((uint) DateTime.Now.Ticks);
                    //I am cheating a bit with the MouseToPosition function, but for the purposes of this demo
                    //creating a Camera Entity was an overkill
                    if (UnityUtilities.MouseToPosition(out Vector3 position))
                    {
                        //BuildEntity returns an EntityInitialized that is used to set the default values of the
                        //entity that will be built.
                        for (int i = 0; i < 500; i++)
                        {
                            EntityComponentInitializer init;

                            var newposition = new float3(position.x + _random.NextFloat(-50, 50), position.y,
                                                         position.z + _random.NextFloat(-50, 50));

                            bool isRed;

                            if (Input.GetMouseButton(0))
                            {
                                init = _entityFactory.BuildEntity<FoodEntityDescriptor>(_foodPlaced++,
                                                        GroupCompound<GameGroups.FOOD, GameGroups.RED, GameGroups.NOTEATING>.BuildGroup);

                                isRed = true;
                            }
                            else
                            {
                                init = _entityFactory.BuildEntity<FoodEntityDescriptor>(_foodPlaced++,
                                                         GroupCompound<GameGroups.FOOD, GameGroups.BLUE, GameGroups.NOTEATING>.BuildGroup);

                                isRed = false;
                            }

                            init.Init(new PositionEntityComponent
                            {
                                position = newposition
                            });
                            //these structs are used for ReactOnAdd callback to create unity Entities later
                            init.Init(new UnityEcsEntityComponent
                            {
                                uecsEntity    = isRed ? _redfood : _bluefood,
                                spawnPosition = newposition,
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
        
        public static readonly SteppableRunner UIInteractionRunner = new SteppableRunner("UIInteraction");

        readonly IEntityFactory _entityFactory;
        readonly Entity         _redfood;
        readonly Entity _bluefood;

        uint _foodPlaced;

        public JobHandle Execute(JobHandle _jobHandle)
        {
            UIInteractionRunner.Step();

            return _jobHandle;
        }
    }
}