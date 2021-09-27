using Stride.Core.Mathematics;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class BulletFactory : IBulletFactory
    {
        public BulletFactory
            (ECSStrideEntityManager ecsStrideEntityManager, IEntityFactory factory)
        {
            _ecsStrideEntityManager = ecsStrideEntityManager;
            _factory                = factory;
        }

        public void LoadBullet()
        {
            _bulletPrefabID = _ecsStrideEntityManager.LoadAndRegisterPrefab("Bullet", out var turretStrideEntityTransform);

            turretStrideEntityTransform.Decompose(out _bulletScaling, out Quaternion _, out _);
        }

        public EntityInitializer CreateBullet()
        {
            var entityResourceID = _ecsStrideEntityManager.InstantiateEntity(_bulletPrefabID, false);
            var init             = _factory.BuildEntity<BulletEntityDescriptor>(entityResourceID, BulletTag.BuildGroup);

            init.Init(new ScalingComponent()
            {
                scaling = _bulletScaling
            });

            return init;
        }

        readonly ECSStrideEntityManager _ecsStrideEntityManager;
        readonly IEntityFactory         _factory;
        Vector3                         _bulletScaling;
        uint                            _bulletPrefabID;
    }

    public interface IBulletFactory
    {
        EntityInitializer CreateBullet();
    }
}