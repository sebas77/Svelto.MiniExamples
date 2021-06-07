using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    class BuildPlayerBotEngine : StartupScript, IEngine
    {
        public BuildPlayerBotEngine(ECSStrideEntityManager ecsStrideEntityManager, IEntityFactory entityFactory)
        {
            _ecsStrideEntityManager = ecsStrideEntityManager;
            _entityFactory         = entityFactory;
        }

        public override void Start()
        {
            var mainBot = Content.Load<Prefab>("StandsPrefabs/Hover");

            var mainBotEntity    = mainBot.Entities[0];
            var entityResourceID = _ecsStrideEntityManager.RegisterStrideEntity(mainBotEntity);

            var init = _entityFactory.BuildEntity<PlayerBotEntityDescriptor>(entityResourceID, PlayerBotTag.BuildGroup);
            init.Init(new TRSComponent(Vector3.Zero, Quaternion.Identity, new Vector3(0.3f, 0.3f, 0.3f)));
            
            SceneSystem.SceneInstance.RootScene.Entities.Add(mainBotEntity);

            mainBotEntity.Transform.UseTRS = false;
        }

        readonly ECSStrideEntityManager _ecsStrideEntityManager;
        readonly IEntityFactory         _entityFactory;
    }
}