using FixedMaths;
using Svelto.ECS;
using Svelto.ECS.Schedulers;
using MiniExamples.DeterministicPhysicDemo.Graphics;
using MiniExamples.DeterministicPhysicDemo.Physics;
using MiniExamples.DeterministicPhysicDemo.Physics.Builders;
using MiniExamples.DeterministicPhysicDemo.Physics.Engines;

namespace MiniExamples.DeterministicPhysicDemo
{
    public class CompositionRoot
    {
        public CompositionRoot(IGraphics graphics)
        {
            _simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            _graphics          = graphics;
            _schedulerReporter = new EngineSchedulerReporter();
            _scheduler         = new EngineScheduler(_schedulerReporter, _simpleSubmissionEntityViewScheduler);

            var enginesRoot     = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            var entityFactory   = enginesRoot.GenerateEntityFactory();

            var debugPhysicsDrawEngine = new DebugPhysicsDrawEngine(_graphics);
            enginesRoot.AddEngine(debugPhysicsDrawEngine);
            scheduler.RegisterScheduledGraphicsEngine(debugPhysicsDrawEngine);

            PhysicsCore.RegisterTo(enginesRoot, _scheduler);

            AddEntities(entityFactory, _simpleSubmissionEntityViewScheduler);
        }
        public IEngineSchedulerReporter schedulerReporter => _schedulerReporter;
        public IEngineScheduler scheduler => _scheduler;

        static void AddEntities(in IEntityFactory entityFactory, in SimpleEntitiesSubmissionScheduler simpleSubmissionEntityViewScheduler)
        {
            // Make a simple bounding box
            RigidBodyWithColliderBuilder.Create()
                                        .SetPosition(FixedPointVector2.From(FixedPoint.From(0), FixedPoint.From(-100)))
                                        .SetBoxCollider(FixedPointVector2.From(100, 5))
                                        .SetIsKinematic(true)
                                        .Build(entityFactory);
            
            RigidBodyWithColliderBuilder.Create()
                                        .SetPosition(FixedPointVector2.From(FixedPoint.From(0), FixedPoint.From(100)))
                                        .SetBoxCollider(FixedPointVector2.From(100, 5))
                                        .SetIsKinematic(true)
                                        .Build(entityFactory);
            
            RigidBodyWithColliderBuilder.Create()
                                        .SetPosition(FixedPointVector2.From(FixedPoint.From(-100), FixedPoint.From(0)))
                                        .SetBoxCollider(FixedPointVector2.From(5, 100))
                                        .SetIsKinematic(true)
                                        .Build(entityFactory);
            
            RigidBodyWithColliderBuilder.Create()
                                        .SetPosition(FixedPointVector2.From(FixedPoint.From(100), FixedPoint.From(0)))
                                        .SetBoxCollider(FixedPointVector2.From(5, 100))
                                        .SetIsKinematic(true)
                                        .Build(entityFactory);
            
            // Add some bounding boxes
            AddBoxColliderEntity(entityFactory, 4, FixedPointVector2.From(FixedPoint.From(-30), FixedPoint.From(0)), FixedPointVector2.Down, FixedPoint.From(3), FixedPointVector2.From(10, 10));
            AddBoxColliderEntity(entityFactory, 5, FixedPointVector2.From(FixedPoint.From(-35), FixedPoint.From(-50)), FixedPointVector2.Up, FixedPoint.From(5), FixedPointVector2.From(10, 10));
            AddBoxColliderEntity(entityFactory, 6, FixedPointVector2.From(FixedPoint.From(-30), FixedPoint.From(50)), FixedPointVector2.Up, FixedPoint.From(3), FixedPointVector2.From(10, 10));
            AddBoxColliderEntity(entityFactory, 7, FixedPointVector2.From(FixedPoint.From(0), FixedPoint.From(50)), FixedPointVector2.Right, FixedPoint.From(3), FixedPointVector2.From(10, 10));
            AddBoxColliderEntity(entityFactory, 8, FixedPointVector2.From(FixedPoint.From(40), FixedPoint.From(-90)), FixedPointVector2.From(1, 1).Normalize(), FixedPoint.From(10), FixedPointVector2.From(3, 3));
            AddBoxColliderEntity(entityFactory, 9, FixedPointVector2.From(FixedPoint.From(40), FixedPoint.From(-60)), FixedPointVector2.From(1, 1).Normalize(), FixedPoint.From(10), FixedPointVector2.From(3, 3));
            AddBoxColliderEntity(entityFactory, 10, FixedPointVector2.From(FixedPoint.From(40), FixedPoint.From(-30)), FixedPointVector2.From(1, 1).Normalize(), FixedPoint.From(3), FixedPointVector2.From(3, 3));

            //this is wrong, SubmitEntities() must happen every tick, not just once, otherwise new entities cannot be submitted
            simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        private static void AddBoxColliderEntity(in IEntityFactory entityFactory, in uint egid, in FixedPointVector2 position, in FixedPointVector2 direction, in FixedPoint speed, in FixedPointVector2 boxColliderSize)
        {
            RigidBodyWithColliderBuilder.Create()
                                        .SetPosition(position)
                                        .SetDirection(direction)
                                        .SetSpeed(speed)
                                        .SetBoxCollider(boxColliderSize)
                                        .Build(entityFactory);
        }

        readonly EngineScheduler          _scheduler;
        readonly EngineSchedulerReporter  _schedulerReporter;
        readonly IGraphics                _graphics;
        
        SimpleEntitiesSubmissionScheduler _simpleSubmissionEntityViewScheduler;
    }
}