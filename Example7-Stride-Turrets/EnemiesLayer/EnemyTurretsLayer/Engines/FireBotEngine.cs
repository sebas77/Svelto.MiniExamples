using System;
using Stride.Core.Mathematics;
using Svelto.Common.Internal;
using Svelto.ECS.MiniExamples.Turrets.BulletLayer;
using Svelto.ECS.MiniExamples.Turrets.PhysicLayer;

namespace Svelto.ECS.MiniExamples.Turrets.EnemyLayer
{
    /// <summary>
    /// Iterate all the bots and if enough time is passed, let them shoot. The bullet is initialised using
    /// the current direction the bot is aiming at.
    /// </summary>
    class FireBotEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public FireBotEngine(IBulletFactory bulletFactory) { _bulletFactory = bulletFactory; }
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public string name => this.TypeName();
        
        public void Step(in float deltaTime)
        {
            foreach (var ((shooting, directions, matrices, count), _) in entitiesDB
               .QueryEntities<ShootingComponent, LookAtComponent, MatrixComponent>(BotTag.Groups))
                for (var i = 0; i < count; i++)
                {
                    shooting[i].time += (float)deltaTime / 1000.0f;   
                    if (shooting[i].time > shooting[i].randomTime)
                    {
                        var init = _bulletFactory.CreateBullet();
                        init.Init(new DirectionComponent()
                        {
                            vector = directions[i].vector
                        });
                        init.Init(new SpeedComponent()
                        {
                            value = 1.0f
                        });
                        init.Init(new PositionComponent()
                        {
                            position = matrices[i].matrix.TranslationVector + new Vector3(0, .12f, 0)
                        });

                        shooting[i].time       = 0;
                        shooting[i].randomTime = _rand.Next(2, 5);
                    }
                }
        }

        readonly IBulletFactory _bulletFactory;
        readonly Random         _rand = new Random();
    }
}