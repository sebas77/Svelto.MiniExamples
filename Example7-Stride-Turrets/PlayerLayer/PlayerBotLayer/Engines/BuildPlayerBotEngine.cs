using Stride.Core.Mathematics;
using Stride.Engine;
using Svelto.ECS.MiniExamples.Turrets.PhysicLayer;
using Svelto.ECS.MiniExamples.Turrets.StrideLayer;

namespace Svelto.ECS.MiniExamples.Turrets.Player
{
    class BuildPlayerBotEngine : IGetReadyEngine
    {
        public BuildPlayerBotEngine
            (ECSStrideEntityManager ecsStrideEntityManager, IEntityFactory entityFactory, SceneSystem sceneSystem)
        {
            _ecsStrideEntityManager = ecsStrideEntityManager;
            _entityFactory          = entityFactory;
            _sceneSystem            = sceneSystem;
        }

        public void Ready()
        {
            var mainBotPrefabID  = _ecsStrideEntityManager.LoadAndRegisterPrefab("StandsPrefabs/Hover", out _);
            var entityResourceID = _ecsStrideEntityManager.InstantiateEntity(mainBotPrefabID, false);
            var mainBotEntity    = _ecsStrideEntityManager.GetStrideEntity(entityResourceID);

            var init = _entityFactory.BuildEntity<PlayerBotEntityDescriptor>(entityResourceID, PlayerBotTag.BuildGroup);
            init.Init(new RotationComponent(Quaternion.Identity));
            init.Init(new ScalingComponent(new Vector3(0.3f, 0.3f, 0.3f)));
            init.Init(new DirectionComponent()
            {
                vector = Vector3.UnitX
            });

            _sceneSystem.SceneInstance.RootScene.Entities.Add(mainBotEntity);
        }

        readonly ECSStrideEntityManager _ecsStrideEntityManager;
        readonly IEntityFactory         _entityFactory;
        readonly SceneSystem            _sceneSystem;
    }
}