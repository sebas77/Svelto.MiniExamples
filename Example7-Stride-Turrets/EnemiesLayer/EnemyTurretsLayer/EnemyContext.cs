using System;
using Svelto.ECS.MiniExamples.Turrets.BulletLayer;
using Svelto.ECS.MiniExamples.Turrets.StrideLayer;

namespace Svelto.ECS.MiniExamples.Turrets.EnemyLayer
{
    public static class EnemyContext
    {
        public static void Compose(Action<IEngine> AddEngine, ECSStrideEntityManager ecsStrideEntityManager, EnginesRoot enginesRoot)
        {
            var bulletFactory = new BulletFactory(ecsStrideEntityManager, enginesRoot.GenerateEntityFactory());
            bulletFactory.LoadBullet();

            AddEngine(new MoveTurretEngine());
            AddEngine(new AimBotEngine());
            AddEngine(new FireBotEngine(bulletFactory));
        }
    }
}