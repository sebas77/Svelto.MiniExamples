using System;
using Stride.Engine;
using Svelto.ECS.MiniExamples.Turrets.StrideLayer;

namespace Svelto.ECS.MiniExamples.Turrets.BulletLayer
{
    public  static class BulletContext
    {
        public static void Compose(Action<IEngine> AddEngine, ECSStrideEntityManager ecsStrideEntityManager, EnginesRoot enginesRoot, SceneSystem sceneSystem)
        {
            var bulletFactory = new BulletFactory(ecsStrideEntityManager, enginesRoot.GenerateEntityFactory());
            bulletFactory.LoadBullet();
            
            AddEngine(new BulletSpawningEngine(ecsStrideEntityManager, sceneSystem));
            AddEngine(new BulletLifeEngine(enginesRoot.GenerateEntityFunctions()));
        }
    }
}