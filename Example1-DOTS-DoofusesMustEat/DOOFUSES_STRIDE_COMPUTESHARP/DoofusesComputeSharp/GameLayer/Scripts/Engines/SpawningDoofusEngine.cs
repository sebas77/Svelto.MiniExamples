using System;
using Stride.Core.Mathematics;
using Stride.Engine;
using Svelto.Common;
using Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    [Sequenced(nameof(DoofusesEngineNames.SpawningDoofusEngine))]
    public class SpawningDoofusEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public SpawningDoofusEngine(uint redCapsule, uint blueCapsule, IEntityFactory factory,
            ECSStrideEntityManager manager, SceneSystem sceneSystem)
        {
            _redCapsule       = redCapsule;
            _blueCapsule      = blueCapsule;
            _factory          = factory;
            _manager          = manager;
            _sceneSystem = sceneSystem;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Step(in float _param)
        {
            if (_done == true)
                return;

            // new SpawningJob()
            // {
            //     _group   = GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup
            //   , _factory = _factory
            //   , _entity  = _redCapsule
            //   , _random  = new Random(1234567)
            // }.Execute(MaxNumberOfDoofuses);

            new SpawningJob()
            {
                _group   = GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup
              , _factory = _factory
              , _prefab  = _blueCapsule
              , _random  = new Random(7654321)
              , _manager = _manager
              , _sceneSystem = _sceneSystem
            }.Execute();

            //Yeah this shouldn't be solved like this, but I keep it in this way for simplicity sake 
            _done = true;    
        }

        public string     name => nameof(SpawningDoofusEngine);

        public void Ready() { }

        readonly IEntityFactory         _factory;
        readonly ECSStrideEntityManager _manager;
        readonly SceneSystem            _sceneSystem;
        readonly uint                   _redCapsule;
        readonly uint                   _blueCapsule;

        const int MaxNumberOfDoofuses = 10000;

        bool _done;

        struct SpawningJob
        {
            internal IEntityFactory         _factory;
            internal uint                   _prefab;
            internal ExclusiveBuildGroup    _group;
            internal Random                 _random;
            public   ECSStrideEntityManager _manager;
            public   SceneSystem            _sceneSystem;

            public void Execute()
            {
                for (var index = 0; index < MaxNumberOfDoofuses; index++)
                {
                    uint entity       = _manager.InstantiateEntity(_prefab, false);
                    
                    var  strideEntity = _manager.GetStrideEntity(entity);
                    _sceneSystem.SceneInstance.RootScene.Entities.Add(strideEntity);
                    var init = _factory.BuildEntity<DoofusEntityDescriptor>(entity, _group);

                    init.Init(new PositionComponent()
                    {
                        position = new Vector3(_random.Next(0, 40), 0, _random.Next(0, 40))
                    });
                    init.Init(new RotationComponent(Quaternion.Identity));
                    init.Init(new ScalingComponent(new Vector3(1.0f, 1.0f, 1.0f)));
                    init.Init(new SpeedEntityComponent
                    {
                        speed = (float)(_random.NextDouble() + 0.1f)
                    });
                }
            }
        }
    }
}