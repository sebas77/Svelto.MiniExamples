using System;
using Svelto.ECS.MiniExamples.Turrets.BulletLayer;

namespace Svelto.ECS.MiniExamples.Turrets.EnemyLayer
{
    public static class EnemyContext
    {
        public static void Compose
            (Action<IEngine> AddEngine, BulletFactory bulletFactory)
        {
            AddEngine(new MoveTurretEngine());
            AddEngine(new AimBotEngine());
            AddEngine(new FireBotEngine(bulletFactory));
        }
    }
}