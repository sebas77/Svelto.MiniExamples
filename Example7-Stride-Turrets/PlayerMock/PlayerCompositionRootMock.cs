using System;
using Stride.Engine;
using Stride.Games;
using Svelto.ECS.MiniExamples.Turrets.PhysicLayer;
using Svelto.ECS.MiniExamples.Turrets.StrideLayer;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.MiniExamples.Turrets.Player
{
    public class PlayerCompositionRootMock : Game
    {
        public PlayerCompositionRootMock()
        {
            GameStarted += CreateCompositionRoot;
        }

        void CreateCompositionRoot(object sender, EventArgs e)
        {
            //Create a SimpleSubmission scheduler to take control over the entities submission ticking
            _scheduler = new SimpleEntitiesSubmissionScheduler();
            //create the engines root
            _enginesRoot = new EnginesRoot(_scheduler);
            //create the Manager that interfaces Stride Objects with Svelto Entities. All the non ECS dependencies
            //like the manager, must be created by the main composition root and injected in the parent composition
            //roots if required.
            _ecsStrideEntityManager = new ECSStrideEntityManager(Content);

            var entityFactory = _enginesRoot.GenerateEntityFactory();

            _mainEngineGroup = new TurretsMainEnginesGroup();

            //Stride Services object is a simple Service Locator Provider.
            //EntityFactory and ecsStrideEnttiyManager can be fetched by Stride systems through
            //the service Locator once they are registered.
            //There is a 1:1 relationship between the Game object and the Services, this means that if multiple
            //engines roots per Game need to be used, a different approach may be necessary. 
            Services.AddService(entityFactory);
            Services.AddService(_ecsStrideEntityManager);

            GameStarted -= CreateCompositionRoot;
        }

        void AddEngine<T>(T engine) where T : class, IEngine
        {
            _enginesRoot.AddEngine(engine);
            if (engine is IUpdateEngine updateEngine)
                _mainEngineGroup.Add(updateEngine);
        }

        protected override void BeginRun()
        {
            TransformableContext.Compose(AddEngine);
            SimplePhysicContext.Compose(AddEngine);
            StrideAbstractionContext.Compose(AddEngine, _ecsStrideEntityManager);
            PlayerContext.Compose(AddEngine, this.Input, _ecsStrideEntityManager, _enginesRoot, SceneSystem);
        }

        protected override void Update(GameTime gameTime)
        {
            //submit entities
            _scheduler.SubmitEntities();
            //run stride logic
            base.Update(gameTime);
            //step the Svelto game engines. We are taking control over the ticking system
            _mainEngineGroup.Step(gameTime.Elapsed.Milliseconds);
        }

        protected override void Destroy()
        {
            _ecsStrideEntityManager.Dispose();
        }

        EnginesRoot                       _enginesRoot;
        SimpleEntitiesSubmissionScheduler _scheduler;
        TurretsMainEnginesGroup           _mainEngineGroup;
        ECSStrideEntityManager            _ecsStrideEntityManager;

        class TurretsMainEnginesGroup : UnsortedEnginesGroup<IUpdateEngine, float> { }
    }
}