using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class BulletSpawningEngine : IGetReadyEngine, IReactOnAddAndRemove<BulletComponent>
    {
        public BulletSpawningEngine
            (BulletFactory bulletFactory, ECSStrideEntityManager ecsStrideEntityManager, SceneSystem sceneSystem)
        {
            _bulletFactory          = bulletFactory;
            _ecsStrideEntityManager = ecsStrideEntityManager;
            _sceneSystem            = sceneSystem;
        }

        public void Ready()
        {
            _bulletFactory.LoadBullet();
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

        readonly BulletFactory          _bulletFactory;
        readonly ECSStrideEntityManager _ecsStrideEntityManager;
        readonly SceneSystem            _sceneSystem;
    }
}