using FixedMaths;
using Svelto.ECS;
using Svelto.ECS.Schedulers;
using MiniExamples.DeterministicPhysicDemo.Graphics;
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
            //create the standard Svelto simple scheduler to submit entities
            _simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            _graphics = graphics;
            _schedulerReporter = new EngineSchedulerReporter();
            _scheduler = new EngineScheduler(_schedulerReporter, _simpleSubmissionEntityViewScheduler);

            //create Svelto.ECS engines root
            var enginesRoot = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            //get the entity factory from it
            var entityFactory = enginesRoot.GenerateEntityFactory();

            //create the svelto engines
            var debugPhysicsDrawEngine = new DebugPhysicsDrawEngine(_graphics);
            var applyVelocityToRigidBodiesEngine = new ApplyVelocityToRigidBodiesEngine();
            var detectDynamicBoxVsBoxCollisionsEngine = new DynamicBoxVsBoxCollisionsEngine();
            var detectDynamicVsKinematicBoxVsBoxCollisionsEngine = new DynamicVsKinematicBoxVsBoxCollisionsEngine();

            //add engines to the engines root
            enginesRoot.AddEngine(debugPhysicsDrawEngine);
            enginesRoot.AddEngine(applyVelocityToRigidBodiesEngine);
            enginesRoot.AddEngine(detectDynamicBoxVsBoxCollisionsEngine);
            enginesRoot.AddEngine(detectDynamicVsKinematicBoxVsBoxCollisionsEngine);

            //Svelto.ECS does not tick engines, engines tick is a solely user responsability
            scheduler.RegisterScheduledGraphicsEngine(debugPhysicsDrawEngine);
            scheduler.RegisterScheduledPhysicsEngine(applyVelocityToRigidBodiesEngine);
            scheduler.RegisterScheduledPhysicsEngine(detectDynamicBoxVsBoxCollisionsEngine);
            scheduler.RegisterScheduledPhysicsEngine(
                detectDynamicVsKinematicBoxVsBoxCollisionsEngine);

            //Entities can be created in the composition root, but it's a strategy ok only for simple scenarios.
            //Factories are composition roots too and usually are used by engines to spawn entities at runtime
            AddEntities(entityFactory);
            
            //it's not time to submit the entities. Ticking the entities submission is also a user responsability 
            //doing int once is ok for this demo only because entities are not submitted at run time, SubmitEntities()
            //must normally happen every tick, not just once, otherwise new entities cannot be submitted
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }

        public IEngineSchedulerReporter schedulerReporter => _schedulerReporter;
        public IEngineScheduler scheduler => _scheduler;

        //Note RigidBodyWithColliderBuilder is a sort of factory to create entities, inside you will find
        //the Svelto.ECS code to build entities
        static void AddEntities(in IEntityFactory entityFactory)
        {
            // Create the 4 bounding boxes acting as edges
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
            RigidBodyWithColliderBuilder.Create()
                   .SetPosition(FixedPointVector2.From(FixedPoint.From(-30), FixedPoint.From(0)))
                   .SetDirection(FixedPointVector2.Down).SetSpeed(FixedPoint.From(3))
                   .SetBoxCollider(FixedPointVector2.From(10, 10)).Build(entityFactory);
            RigidBodyWithColliderBuilder.Create()
                   .SetPosition(FixedPointVector2.From(FixedPoint.From(-35), FixedPoint.From(-50)))
                   .SetDirection(FixedPointVector2.Up).SetSpeed(FixedPoint.From(5))
                   .SetBoxCollider(FixedPointVector2.From(10, 10)).Build(entityFactory);
            RigidBodyWithColliderBuilder.Create()
                   .SetPosition(FixedPointVector2.From(FixedPoint.From(-30), FixedPoint.From(50)))
                   .SetDirection(FixedPointVector2.Up).SetSpeed(FixedPoint.From(3))
                   .SetBoxCollider(FixedPointVector2.From(10, 10)).Build(entityFactory);
            RigidBodyWithColliderBuilder.Create()
                   .SetPosition(FixedPointVector2.From(FixedPoint.From(0), FixedPoint.From(50)))
                   .SetDirection(FixedPointVector2.Right).SetSpeed(FixedPoint.From(3))
                   .SetBoxCollider(FixedPointVector2.From(10, 10)).Build(entityFactory);
            RigidBodyWithColliderBuilder.Create()
                   .SetPosition(FixedPointVector2.From(FixedPoint.From(40), FixedPoint.From(-90)))
                   .SetDirection(FixedPointVector2.From(1, 1).Normalize()).SetSpeed(FixedPoint.From(10))
                   .SetBoxCollider(FixedPointVector2.From(3, 3)).Build(entityFactory);
            RigidBodyWithColliderBuilder.Create()
                   .SetPosition(FixedPointVector2.From(FixedPoint.From(40), FixedPoint.From(-60)))
                   .SetDirection(FixedPointVector2.From(1, 1).Normalize()).SetSpeed(FixedPoint.From(10))
                   .SetBoxCollider(FixedPointVector2.From(3, 3)).Build(entityFactory);
            RigidBodyWithColliderBuilder.Create()
                   .SetPosition(FixedPointVector2.From(FixedPoint.From(40), FixedPoint.From(-30)))
                   .SetDirection(FixedPointVector2.From(1, 1).Normalize()).SetSpeed(FixedPoint.From(3))
                   .SetBoxCollider(FixedPointVector2.From(3, 3)).Build(entityFactory);
        }

        readonly EngineScheduler _scheduler;
        readonly EngineSchedulerReporter _schedulerReporter;
        readonly IGraphics _graphics;

        SimpleEntitiesSubmissionScheduler _simpleSubmissionEntityViewScheduler;
    }
}