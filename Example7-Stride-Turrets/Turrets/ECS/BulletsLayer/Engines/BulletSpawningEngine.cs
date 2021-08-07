using System.Threading.Tasks;
using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class BulletSpawningEngine : StartupScript, IReactOnAddAndRemove<BulletComponent>
    {
        public BulletSpawningEngine(BulletFactory bulletFactory, ECSStrideEntityManager ecsStrideEntityManager)
        {
            _bulletFactory          = bulletFactory;
            _ecsStrideEntityManager = ecsStrideEntityManager;
        }

        public override void Start() { _bulletFactory.Init(Content); }

        public void Add(ref BulletComponent entityComponent, EGID egid)
        {
            SceneSystem.SceneInstance.RootScene.Entities.Add(
                _ecsStrideEntityManager.GetStrideEntity(egid.entityID));
        }

        public void Remove(ref BulletComponent entityComponent, EGID egid)
        {
            SceneSystem.SceneInstance.RootScene.Entities.Remove(
                _ecsStrideEntityManager.GetStrideEntity(egid.entityID));
        }

        readonly BulletFactory          _bulletFactory;
        readonly ECSStrideEntityManager _ecsStrideEntityManager;
    }
}