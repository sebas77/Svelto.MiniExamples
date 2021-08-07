using System;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.Games;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class TurretsCompositionRoot : Game
    {
        public TurretsCompositionRoot()
        {
            GameStarted += CreateCompositionRoot;
        }

        void CreateCompositionRoot(object sender, EventArgs e)
        { 
             _scheduler   = new SimpleEntitiesSubmissionScheduler();
             _enginesRoot = new EnginesRoot(_scheduler);
            
             var ecsStrideEntityManager  = new ECSStrideEntityManager();
             var entityFactory           = _enginesRoot.GenerateEntityFactory();
             var entityFunctions = _enginesRoot.GenerateEntityFunctions();
            
             Services.AddService(entityFactory);
             Services.AddService(entityFunctions);
             Services.AddService(ecsStrideEntityManager);
             
             var bulletFactory = new BulletFactory(ecsStrideEntityManager, entityFactory);

             //Player Context
             AddEngine(new PlayerBotInputEngine());
             AddEngine(new BuildPlayerBotEngine(ecsStrideEntityManager, entityFactory));
            
             //TurretsContext
             AddEngine(new MoveTurretEngine());
             AddEngine(new AimBotEngine());
             AddEngine(new FireBotEngine(bulletFactory));
             
             //BulletsContext
             AddEngine(new BulletSpawningEngine(bulletFactory, ecsStrideEntityManager));
            
             //SimplePhysicContext
             AddEngine(new VelocityComputationEngine());
             AddEngine(new VelocityToPositionEngine());
            
             //TransformableContext
             AddEngine(new LookAtEngine());
             AddEngine(new ComputeTransformsEngine());
             //HierarchicalTransformableContext
             AddEngine(new ComputeHierarchicalTransformsEngine());
            
             //Stride Abstraction Layer
             AddEngine(new SetTransformsEngine(ecsStrideEntityManager));
            
            GameStarted -= CreateCompositionRoot;
        }

        void AddEngine<T>(T engine) where T:ScriptComponent, IEngine 
        {
            Script.Add(engine);
            _enginesRoot.AddEngine(engine);
        }

        protected override void Update(GameTime gameTime)
        {
            //submit entities
            _scheduler.SubmitEntities();
            //run stride logic
            base.Update(gameTime);
        }

        EnginesRoot                       _enginesRoot;
        SimpleEntitiesSubmissionScheduler _scheduler;
    }
}