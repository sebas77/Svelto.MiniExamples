using System;
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
            foreach (var ((shooting, count), _) in entitiesDB.QueryEntities<ShootingComponent>(BotTag.Groups))
                for (var i = 0; i < count; i++)
                    if ((DateTime.Now - shooting[i].time).TotalSeconds > 5 && _bulletsFired == 0)
                    {
                        _bulletsFired++;
                            
                        _bulletFactory.CreateBullet();

                        shooting[i].time = DateTime.Now;
                    }
        }

        readonly IBulletFactory _bulletFactory;

        byte _bulletsFired;
    }
}