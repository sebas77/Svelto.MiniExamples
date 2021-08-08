using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Engine;

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

        public void Init(IContentManager contentManager)
        {
            var prefab = contentManager.Load<Prefab>("Bullet");

            _bulletPrefab = prefab.Entities[0];

            var turretStrideEntityTransform = _bulletPrefab.Transform;

            turretStrideEntityTransform.LocalMatrix.Decompose(out _bulletScaling, out Quaternion _, out _);
        }

        public EntityInitializer CreateBullet()
        {
            var bullet           = _bulletPrefab.Clone();
            var entityResourceID = _ecsStrideEntityManager.RegisterStrideEntity(bullet);
            var init             = _factory.BuildEntity<BulletEntityDescriptor>(entityResourceID, BulletTag.BuildGroup);

            init.Init(new ScalingComponent()
            {
                scaling = _bulletScaling
            });

            bullet.Transform.UseTRS = false;

            return init;
        }

        readonly ECSStrideEntityManager _ecsStrideEntityManager;
        Entity                          _bulletPrefab;
        readonly IEntityFactory         _factory;
        Vector3                         _bulletScaling;
    }

    public interface IBulletFactory
    {
        EntityInitializer CreateBullet();
    }
}