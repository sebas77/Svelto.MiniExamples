using Stride.Core.Mathematics;
using Stride.Engine;
using Svelto.ECS.MiniExamples.Turrets.PhysicLayer;
using Svelto.ECS.MiniExamples.Turrets.StrideLayer;

namespace Svelto.ECS.MiniExamples.Turrets.Player
{
    public static class PlayerFactory
    {
        public static void CreatePlayerEntity<T>
            (ECSStrideEntityManager ecsStrideEntityManager, SceneSystem sceneSystem, IEntityFactory factory)
            where T : ExtendibleEntityDescriptor<PlayerBotEntityDescriptor>, new()
        {
            var mainBotPrefabID  = ecsStrideEntityManager.LoadAndRegisterPrefab("StandsPrefabs/Hover", out _);
            var entityResourceID = ecsStrideEntityManager.InstantiateEntity(mainBotPrefabID, false);
            var mainBotEntity    = ecsStrideEntityManager.GetStrideEntity(entityResourceID);

            sceneSystem.SceneInstance.RootScene.Entities.Add(mainBotEntity);

            var init = factory.BuildEntity<T>(entityResourceID, PlayerBotTag.BuildGroup);

            init.Init(new RotationComponent(Quaternion.Identity));
            init.Init(new ScalingComponent(new Vector3(0.3f, 0.3f, 0.3f)));
            init.Init(new DirectionComponent()
            {
                vector = Vector3.UnitX
            });
        }
    }
}