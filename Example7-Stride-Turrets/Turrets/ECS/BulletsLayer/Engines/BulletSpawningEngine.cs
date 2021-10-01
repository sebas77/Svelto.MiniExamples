using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    /// <summary>
    /// when Bullet Entities are added, enable the relative stride entities
    /// </summary>
    class BulletSpawningEngine : IReactOnAddAndRemove<BulletComponent>
    {
        public BulletSpawningEngine(ECSStrideEntityManager ecsStrideEntityManager, SceneSystem sceneSystem)
        {
            _ecsStrideEntityManager = ecsStrideEntityManager;
            _sceneSystem            = sceneSystem;
        }

        public void Add(ref BulletComponent entityComponent, EGID egid)
        {
            _sceneSystem.SceneInstance.RootScene.Entities.Add(_ecsStrideEntityManager.GetStrideEntity(egid.entityID));
        }

        public void Remove(ref BulletComponent entityComponent, EGID egid)
        {
            _sceneSystem.SceneInstance.RootScene.Entities.Remove(
                _ecsStrideEntityManager.GetStrideEntity(egid.entityID));
        }

        readonly ECSStrideEntityManager _ecsStrideEntityManager;
        readonly SceneSystem            _sceneSystem;
    }
}