using System;
using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    [Sequenced(nameof(DoofusesEngineNames.SpawningDoofusEngine))]
    public class SpawningDoofusEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public SpawningDoofusEngine(uint blueCapsule, uint redCapsule, IEntityFactory factory,
            ECSStrideEntityManager manager)
        {
            _redCapsule       = redCapsule;
            _blueCapsule      = blueCapsule;
            _factory          = factory;
            _manager          = manager;
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Step(in float _param)
        {
            if (_done == true)
                return;
            
            uint blueEntityPrefab = _manager.InstantiateInstancingEntity(_blueCapsule);
            uint redEntityPrefab = _manager.InstantiateInstancingEntity(_redCapsule);

            new SpawningJob()
            {
                _group   = GameGroups.RED_DOOFUSES_NOT_EATING.BuildGroup
              , _factory = _factory, _random = new Random(Environment.TickCount)
              , _entity  = redEntityPrefab
            }.Execute();

            new SpawningJob()
            {
                _group    = GameGroups.BLUE_DOOFUSES_NOT_EATING.BuildGroup
              , _factory  = _factory, _random = new Random(Environment.TickCount)
                , _entity = blueEntityPrefab
            }.Execute();

            //Yeah this shouldn't be solved like this, but I keep it in this way for simplicity sake 
            _done = true;    
        }

        public string     name => nameof(SpawningDoofusEngine);

        public void Ready() { }

        readonly IEntityFactory         _factory;
        readonly ECSStrideEntityManager _manager;
        readonly uint                   _redCapsule;
        readonly uint                   _blueCapsule;

        const int MaxNumberOfDoofuses = 10000;

        bool _done;

        struct SpawningJob
        {
            internal IEntityFactory      _factory;
            internal ExclusiveBuildGroup _group;
            internal Random              _random;
            public   uint                _entity;

            public void Execute()
            {
                for (var index = 0; index < MaxNumberOfDoofuses; index++)
                {
                    var init = _factory.BuildEntity<DoofusEntityDescriptor>((uint)index, _group);

                    init.Init(new PositionComponent()
                    {
                        position = new Vector3((float)(_random.NextDouble() * 40.0f), 0, (float)(_random.NextDouble() * 40.0f))
                    });
                    init.Init(new RotationComponent(Quaternion.Identity));
                    init.Init(new ScalingComponent(new Vector3(1.0f, 1.0f, 1.0f)));
                    init.Init(new SpeedEntityComponent
                    {
                        speed = (float)(_random.NextDouble() + 0.1f)
                    });
                    init.Init(new StrideComponent()
                    {
                        entity = _entity
                    });
                }
            }
        }
    }
}