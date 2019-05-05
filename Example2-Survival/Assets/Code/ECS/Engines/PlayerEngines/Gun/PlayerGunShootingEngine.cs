using System.Collections;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public class PlayerGunShootingEngine
        : IQueryingEntitiesEngine
    {
        readonly IRayCaster                _rayCaster;
        readonly ITime                     _time;

        public PlayerGunShootingEngine(IRayCaster rayCaster, ITime time)
        {
            _rayCaster   = rayCaster;
            _time        = time;
        }

        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready() { Tick().RunOnScheduler(StandardSchedulers.physicScheduler); }

        IEnumerator Tick()
        {
            while (true)
            {
                ///pay attention: the code of this example started in a naive way, it originally assumed that
                /// there was just one player available, which resulting code could have promoted some not
                /// very good practices. However assuming that there is a relationship 1:1 between entities
                /// between groups is just naive. This works because there is just one player
                var playerEntities =
                    entitiesDB.QueryEntities<PlayerInputDataStruct>(ECSGroups.Player, out var count);
                var gunEntities =
                    entitiesDB.QueryEntities<GunEntityViewStruct>(ECSGroups.PlayerGun, out _);
                
                for (var i = 0; i < count; i++)
                {
                    var playerGunComponent = gunEntities[i].gunComponent;
                    playerGunComponent.timer += _time.deltaTime;

                    if (playerEntities[i].fire && playerGunComponent.timer >= playerGunComponent.timeBetweenBullets)
                        Shoot(ref gunEntities[i]);
                }
                
                yield return null;
            }
        }

        /// <summary>
        ///     Design note: shooting and find a target are possibly two different responsibilities
        ///     and probably would need two different engines.
        /// </summary>
        /// <param name="playerGunEntityView"></param>
        void Shoot(ref GunEntityViewStruct playerGunEntityView)
        {
            var playerGunComponent    = playerGunEntityView.gunComponent;
            var playerGunHitComponent = playerGunEntityView.gunHitTargetComponent;

            playerGunComponent.timer = 0;

            var entityHit = _rayCaster.CheckHit(playerGunComponent.shootRay, playerGunComponent.range,
                                                GAME_LAYERS.ENEMY_LAYER,
                                                GAME_LAYERS.SHOOTABLE_MASK | GAME_LAYERS.ENEMY_MASK, out var point,
                                                out var instanceID);

            if (entityHit)
            {
                var damageInfo = new DamageInfo(playerGunComponent.damagePerShot, point);

                //note how the GameObject GetInstanceID is used to identify the entity as well
                if (instanceID != -1)
                    entitiesDB.QueryEntity<DamageableEntityStruct>((uint) instanceID, ECSGroups.PlayerTargets)
                              .damageInfo = damageInfo;

                playerGunComponent.lastTargetPosition = point;
                playerGunHitComponent.targetHit.value = true;
            }
            else
            {
                playerGunHitComponent.targetHit.value = false;
            }
        }
    }
}