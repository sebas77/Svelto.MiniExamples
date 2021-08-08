using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class FireBotEngine : SyncScript, IQueryingEntitiesEngine
    {
        public FireBotEngine(IBulletFactory bulletFactory) { _bulletFactory = bulletFactory; }
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public override void Update()
        {
            foreach (var ((shooting, directions, matrices, count), _) in entitiesDB
               .QueryEntities<ShootingComponent, LookAtComponent, MatrixComponent>(BotTag.Groups))
                for (var i = 0; i < count; i++)
                {
                    shooting[i].time += (float)Game.UpdateTime.Elapsed.TotalSeconds;   
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