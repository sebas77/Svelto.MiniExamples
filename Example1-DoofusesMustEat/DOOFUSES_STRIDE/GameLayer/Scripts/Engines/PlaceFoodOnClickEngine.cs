using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Physics;
using Svelto.Common;
using Svelto.ECS.MiniExamples.Doofuses.StrideExample.StrideLayer;
using Quaternion = Stride.Core.Mathematics.Quaternion;
using Vector3 = Stride.Core.Mathematics.Vector3;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    [Sequenced(nameof(DoofusesEngineNames.PlaceFoodOnClickEngine))]
    public class PlaceFoodOnClickEngine : IQueryingEntitiesEngine, IUpdateEngine, IReactOnRemoveEx<MealInfoComponent>
    {
        const int MaxMeals         = 250;
        const int MaxMealsOnScreen = 10000;

        public PlaceFoodOnClickEngine
        (uint redfood, uint bluefood, IEntityFactory entityFactory, InputManager input, SceneSystem sceneSystem
       , ECSStrideEntityManager manager)
        {
            //read why I am using a Native Factory here in the Execute method
            _entityFactory = entityFactory;
            _input         = input;

            _redfood  = manager.InstantiateInstancingEntity(redfood);
            _bluefood = manager.InstantiateInstancingEntity(bluefood);

            var sceneSystemSceneInstance = sceneSystem.SceneInstance;
            var camera = sceneSystemSceneInstance.RootScene.Entities.First(e => e.Components.Get<CameraComponent>() != null);
            _camera     = camera.Get<CameraComponent>();
            _simulation = sceneSystemSceneInstance.GetProcessor<PhysicsProcessor>().Simulation;
        }

        public void Step(in float _param)
        {
            _taskRunner.MoveNext();
        }

        public void Remove
        ((uint start, uint end) rangeOfEntities, in EntityCollection<MealInfoComponent> collection
       , ExclusiveGroupStruct groupID)
        {
            _foodCounter -= (int)(rangeOfEntities.end - rangeOfEntities.start);
        }

        public string name => nameof(PlaceFoodOnClickEngine);

        IEnumerator CheckClick()
        {
            while (true)
            {
                bool isLeft;
                //note: in a complex project an engine shouldn't ever poll input directly, it should instead poll
                //entity states
                if (((isLeft = _input.IsMouseButtonDown(MouseButton.Left))
                  || _input.IsMouseButtonDown(MouseButton.Middle) == true) && _foodCounter + MaxMeals < MaxMealsOnScreen)
                {
                    //I am cheating a bit with the MouseToPosition function, but for the purposes of this demo
                    //creating a Camera Entity was an overkill
                    if (MouseUtilityClass.ScreenPositionToWorldPositionRaycast(
                            _input.MousePosition, _camera, _simulation, out HitResult result))
                    {
                        if (isLeft)
                        {
                            new PlaceFood(result.Point, _entityFactory, GameGroups.RED_FOOD_NOT_EATEN.BuildGroup
                                        , _redfood, _foodPlaced, new Random(Environment.TickCount)).Execute();
                        }
                        else
                        {
                            new PlaceFood(result.Point, _entityFactory, GameGroups.BLUE_FOOD_NOT_EATEN.BuildGroup
                                        , _bluefood, _foodPlaced, new Random(DateTime.Now.Millisecond)).Execute();
                        }

                        _foodPlaced  += MaxMeals;
                        _foodCounter += MaxMeals;

                        var now = DateTime.Now;
                        while ((DateTime.Now - now).TotalMilliseconds < 100)
                            yield return null;
                    }
                }

                yield return null;
            }
        }

        readonly struct PlaceFood
        {
            readonly Vector3             _position;
            readonly IEntityFactory      _entityFactory;
            readonly ExclusiveBuildGroup _exclusiveBuildGroup;
            readonly uint                _prefabID;
            readonly uint                _foodPlaced;
            readonly Random              _random;

            public PlaceFood
            (Vector3 position, IEntityFactory factory, ExclusiveBuildGroup exclusiveBuildGroup, uint prefabID
           , uint foodPlaced, Random random) : this()
            {
                _position            = position;
                _entityFactory       = factory;
                _exclusiveBuildGroup = exclusiveBuildGroup;
                _prefabID            = prefabID;
                _foodPlaced          = foodPlaced;
                _random              = random;
            }

            public void Execute()
            {
                for (int index = 0; index < MaxMeals; index++)
                {
                    float randX       = (float)(_position.X + ((_random.NextDouble() * 30.0f) - 15.0f));
                    float randZ       = (float)(_position.Z + ((_random.NextDouble() * 30.0f) - 15.0f));
                    var   newposition = new Vector3(randX, _position.Y, randZ);

                    var init = _entityFactory.BuildEntity<FoodEntityDescriptor>(new EGID((uint)(_foodPlaced + index), _exclusiveBuildGroup));

                    var scalingRotation = Quaternion.Identity;
                    var scalingCenter = Vector3.One;
                    Matrix.Transformation(ref scalingCenter, ref scalingRotation, ref newposition, out var matrix);
                    
                    init.Init(new MatrixComponent()
                    {
                        matrix = matrix
                    });
                    
                    init.Init(new PositionComponent()
                    {
                        position = newposition
                    });

                    //these structs are used for ReactOnAdd callback to create unity Entities later
                    init.Init(new StrideComponent()
                    {
                        prefabID = _prefabID,
                        updateOnce =  true
                    });
                }
            }
        }

        public EntitiesDB entitiesDB { private get; set; }

        public void Ready()
        {
            _taskRunner = CheckClick();
        }

        readonly uint _redfood;
        readonly uint _bluefood;
        uint          _foodPlaced;
        int          _foodCounter;

        readonly IEntityFactory  _entityFactory;
        static   IEnumerator     _taskRunner;
        readonly InputManager    _input;
        readonly Simulation      _simulation;
        readonly CameraComponent _camera;
    }
}