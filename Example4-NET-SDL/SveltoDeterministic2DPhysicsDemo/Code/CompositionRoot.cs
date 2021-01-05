using FixedMaths;
using Svelto.ECS;
using Svelto.ECS.Schedulers;
using MiniExamples.DeterministicPhysicDemo.Graphics;
using MiniExamples.DeterministicPhysicDemo.Physics;
using MiniExamples.DeterministicPhysicDemo.Physics.Builders;
using MiniExamples.DeterministicPhysicDemo.Physics.Engines;

namespace MiniExamples.DeterministicPhysicDemo
{
    /// <summary>
    /// Composing Engines Root, Scheduler and other project related objects.
    /// </summary>
    public class CompositionRoot
    {
        public CompositionRoot(IGraphics graphics)
        {
            _simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            _graphics = graphics;
            _schedulerReporter = new EngineSchedulerReporter();
            _scheduler = new EngineScheduler(_schedulerReporter, _simpleSubmissionEntityViewScheduler);

            var enginesRoot   = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            var entityFactory = enginesRoot.GenerateEntityFactory();

            var debugPhysicsDrawEngine = new DebugPhysicsDrawEngine(_graphics);
            enginesRoot.AddEngine(debugPhysicsDrawEngine);
            scheduler.RegisterScheduledGraphicsEngine(debugPhysicsDrawEngine);

            var applyVelocityToRigidBodiesEngine                 = new ApplyVelocityToRigidBodiesEngine();
            var detectDynamicBoxVsBoxCollisionsEngine            = new DynamicBoxVsBoxCollisionsEngine();
            var detectDynamicVsKinematicBoxVsBoxCollisionsEngine = new DynamicVsKinematicBoxVsBoxCollisionsEngine();

            enginesRoot.AddEngine(applyVelocityToRigidBodiesEngine);
            enginesRoot.AddEngine(detectDynamicBoxVsBoxCollisionsEngine);
            enginesRoot.AddEngine(detectDynamicVsKinematicBoxVsBoxCollisionsEngine);

            ((IEngineScheduler) _scheduler).RegisterScheduledPhysicsEngine(applyVelocityToRigidBodiesEngine);
            ((IEngineScheduler) _scheduler).RegisterScheduledPhysicsEngine(detectDynamicBoxVsBoxCollisionsEngine);
            ((IEngineScheduler) _scheduler).RegisterScheduledPhysicsEngine(detectDynamicVsKinematicBoxVsBoxCollisionsEngine);

            //Entities can be created in the composition root, but it's a strategy ok only for simple scenarios.
            //Normally factory engines are used instead.
            AddEntities(entityFactory, _simpleSubmissionEntityViewScheduler);
        }

        public IEngineSchedulerReporter schedulerReporter => _schedulerReporter;
        public IEngineScheduler         scheduler         => _scheduler;

        static void AddEntities
            (in IEntityFactory entityFactory, in SimpleEntitiesSubmissionScheduler simpleSubmissionEntityViewScheduler)
        {
            // Make a simple bounding box
            RigidBodyWithColliderBuilder.Create()
                                        .SetPosition(FixedPointVector2.From(FixedPoint.From(0), FixedPoint.From(-100)))
                                        .SetBoxCollider(FixedPointVector2.From(100, 5)).SetIsKinematic(true)
                                        .Build(entityFactory);

            RigidBodyWithColliderBuilder.Create()
                                        .SetPosition(FixedPointVector2.From(FixedPoint.From(0), FixedPoint.From(100)))
                                        .SetBoxCollider(FixedPointVector2.From(100, 5)).SetIsKinematic(true)
                                        .Build(entityFactory);

            RigidBodyWithColliderBuilder.Create()
                                        .SetPosition(FixedPointVector2.From(FixedPoint.From(-100), FixedPoint.From(0)))
                                        .SetBoxCollider(FixedPointVector2.From(5, 100)).SetIsKinematic(true)
                                        .Build(entityFactory);

            RigidBodyWithColliderBuilder.Create()
                                        .SetPosition(FixedPointVector2.From(FixedPoint.From(100), FixedPoint.From(0)))
                                        .SetBoxCollider(FixedPointVector2.From(5, 100)).SetIsKinematic(true)
                                        .Build(entityFactory);

            // Add some bounding boxes
            AddBoxColliderEntity(entityFactory, FixedPointVector2.From(FixedPoint.From(-30), FixedPoint.From(0))
                               , FixedPointVector2.Down, FixedPoint.From(3), FixedPointVector2.From(10, 10));
            AddBoxColliderEntity(entityFactory, FixedPointVector2.From(FixedPoint.From(-35), FixedPoint.From(-50))
                               , FixedPointVector2.Up, FixedPoint.From(5), FixedPointVector2.From(10, 10));
            AddBoxColliderEntity(entityFactory, FixedPointVector2.From(FixedPoint.From(-30), FixedPoint.From(50))
                               , FixedPointVector2.Up, FixedPoint.From(3), FixedPointVector2.From(10, 10));
            AddBoxColliderEntity(entityFactory, FixedPointVector2.From(FixedPoint.From(0), FixedPoint.From(50))
                               , FixedPointVector2.Right, FixedPoint.From(3), FixedPointVector2.From(10, 10));
            AddBoxColliderEntity(entityFactory, FixedPointVector2.From(FixedPoint.From(40), FixedPoint.From(-90))
                               , FixedPointVector2.From(1, 1).Normalize(), FixedPoint.From(10)
                               , FixedPointVector2.From(3, 3));
            AddBoxColliderEntity(entityFactory, FixedPointVector2.From(FixedPoint.From(40), FixedPoint.From(-60))
                               , FixedPointVector2.From(1, 1).Normalize(), FixedPoint.From(10)
                               , FixedPointVector2.From(3, 3));
            AddBoxColliderEntity(entityFactory, FixedPointVector2.From(FixedPoint.From(40), FixedPoint.From(-30))
                               , FixedPointVector2.From(1, 1).Normalize(), FixedPoint.From(3)
                               , FixedPointVector2.From(3, 3));

            //this is wrong, SubmitEntities() must happen every tick, not just once, otherwise new entities cannot be submitted
            simpleSubmissionEntityViewScheduler.SubmitEntities();
        }

        static void AddBoxColliderEntity
        (in IEntityFactory entityFactory, in FixedPointVector2 position
       , in FixedPointVector2 direction, in FixedPoint speed, in FixedPointVector2 boxColliderSize)
        {
            RigidBodyWithColliderBuilder.Create().SetPosition(position).SetDirection(direction).SetSpeed(speed)
                                        .SetBoxCollider(boxColliderSize).Build(entityFactory);
        }

        readonly EngineScheduler         _scheduler;
        readonly EngineSchedulerReporter _schedulerReporter;
        readonly IGraphics               _graphics;

        SimpleEntitiesSubmissionScheduler _simpleSubmissionEntityViewScheduler;
    }
}