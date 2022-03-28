using System;
using Stride.Engine;
using Stride.Games;
using Svelto.DataStructures;
using Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
{
    public class DoofusesCompositionRoot : Game
    {
        public DoofusesCompositionRoot()
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
            _ecsStrideEntityManager = new ECSStrideEntityManager(Content, SceneSystem);
            
            _unsortedGroups = new FasterList<IUpdateEngine>();

            var entityFactory = _enginesRoot.GenerateEntityFactory();

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
                _unsortedGroups.Add(updateEngine);
        }

        protected override void BeginRun()
        {
            WindowMinimumUpdateRate.MinimumElapsedTime = new TimeSpan(0);
            
            GraphicsDevice.Presenter.PresentInterval = Stride.Graphics.PresentInterval.Immediate;
            
            LoadAssetAndCreatePrefabs(_ecsStrideEntityManager, out var blueDoofusPrefab);

            GameCompositionRoot(blueDoofusPrefab);
        }

        void GameCompositionRoot(uint blueDoofusPrefab)
        {
            var entityFactory = _enginesRoot.GenerateEntityFactory();
            //Compose the game level engines
            //            AddEngine(new PlaceFoodOnClickEngine(redFoodPrefab, blueFootPrefab, entityFactory));
            AddEngine(new SpawningDoofusEngine(blueDoofusPrefab, blueDoofusPrefab, entityFactory,
                _ecsStrideEntityManager));
            //          AddEngine(new ConsumingFoodEngine(entityFunctions));
            //        AddEngine(new LookingForFoodDoofusesEngine(entityFunctions));
            //      AddEngine(new VelocityToPositionDoofusesEngine());

            StrideAbstractionContext.Compose(AddEngine, _ecsStrideEntityManager);

            _mainEngineGroup = new SortedDoofusesEnginesExecutionGroup(_unsortedGroups);
        }

        void LoadAssetAndCreatePrefabs(ECSStrideEntityManager gom
            //        out int redFoodPrefab, 
            //          out int blueFootPrefab, 
            //            out int redDoofusPrefab
          , out uint blueDoofusPrefab)
        {
            //Register the loaded prefabs in the GameObject layer. The returning id is the ECS id to the prefab
            //            redFoodPrefab    = Content.Load<Entity>("entity1");
            //            blueFootPrefab   = Content.Load<Entity>("entity1");
            //redDoofusPrefab  = Content.Load<Entity>("entity1");
            blueDoofusPrefab = gom.LoadAndRegisterPrefab("Capsule_2_p", out _);
        }

        protected override void Update(GameTime gameTime)
        {
            //submit entities
            _scheduler.SubmitEntities();
            //run stride logic
            base.Update(gameTime);
            //step the Svelto game engines. We are taking control over the ticking system
            //Release mode issue: either do the if or remove the splash screen from the settings (texture=null)
            if (SceneSystem.SceneInstance != null)
                _mainEngineGroup.Step(gameTime.Elapsed.Milliseconds);
        }

        protected override void Destroy()
        {
            _ecsStrideEntityManager.Dispose();
        }

        EnginesRoot                         _enginesRoot;
        SimpleEntitiesSubmissionScheduler   _scheduler;
        SortedDoofusesEnginesExecutionGroup _mainEngineGroup;
        ECSStrideEntityManager              _ecsStrideEntityManager;
        FasterList<IUpdateEngine>           _unsortedGroups;
    }
}