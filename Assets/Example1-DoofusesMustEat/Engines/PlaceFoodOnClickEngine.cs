using System.Collections;
using Svelto.ECS.Components;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;
using Svelto.Tasks.ExtraLean;
using Unity.Entities;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class PlaceFoodOnClickEngine : IQueryingEntitiesEngine
    {
        public PlaceFoodOnClickEngine(Entity food, IEntityFactory entityFactory)
        {
            _entityFactory = entityFactory;
            _food = food;
        }

        IEnumerator CheckClick()
        {
            var timer = new ReusableWaitForSecondsEnumerator(0.08f);
            
            while (true)
            {
                if (Input.GetMouseButton(0) == true)
                {
                    if (UnityUtilities.MouseToPosition(out Vector3 position))
                    {
                        var init = _entityFactory.BuildEntity<FoodEntityDescriptor>(_foodPlaced++, GameGroups.FOOD);
                        init.Init(new MealEntityStruct(10000));
                        var positionEntityStruct = new PositionEntityStruct
                        {
                            position = new ECSVector3(position.x, position.y, position.z)
                        };
                        init.Init(ref positionEntityStruct);
                        init.Init(new UnityECSEntityStruct
                        {
                            uecsEntity        = _food,
                            spawnPosition = positionEntityStruct.position,
                            unityComponent = ComponentType.ReadWrite<UnityECSFoodGroup>()
                        });
                    }
                }
                
                while (timer.IsDone() == false)
                    yield return Yield.It;
            }
        }

        public IEntitiesDB entitiesDB { private get; set; }

        public void Ready()
        {
            CheckClick().RunOn(DoofusesStandardSchedulers.UIInteraction);
        }
        
        readonly IEntityFactory _entityFactory;
        readonly Entity         _food;
        
        uint _foodPlaced;
    }
}