using System;
using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.ECS.MiniExamples.Turrets;

namespace Svelto.ECS.MiniExamples.Example1C
{
    [Sequenced(nameof(DoofusesEngineNames.SpawningDoofusEngine))]
    public class SpawningDoofusEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public SpawningDoofusEngine(int redCapsule, int blueCapsule, IEntityFactory factory)
        {
            _redCapsule  = redCapsule;
            _blueCapsule = blueCapsule;
            _factory     = factory;
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
              , _entity  = _blueCapsule
              , _random  = new Random(7654321)
            }.Execute(MaxNumberOfDoofuses);

            //Yeah this shouldn't be solved like this, but I keep it in this way for simplicity sake 
            _done = true;    
        }

        public string     name => nameof(SpawningDoofusEngine);

        public void Ready() { }

        readonly IEntityFactory _factory;
        readonly int            _redCapsule, _blueCapsule;

        const int MaxNumberOfDoofuses = 10000;

        bool _done;

        struct SpawningJob 
        {
            internal IEntityFactory      _factory;
            internal int                 _entity;
            internal ExclusiveBuildGroup _group;
            internal Random              _random;

            public void Execute(int index)
            {
                var positionEntityComponent = new PositionComponent()
                {
                    position = new Vector3(_random.Next(0, 40), 0, _random.Next(0, 40))
                };

                var init = _factory.BuildEntity<DoofusEntityDescriptor>((uint)index, _group);

                init.Init(positionEntityComponent);
                init.Init(new SpeedEntityComponent
                {
                    speed = (float)(_random.NextDouble() + 0.1f)
                });
            }
        }
    }
}