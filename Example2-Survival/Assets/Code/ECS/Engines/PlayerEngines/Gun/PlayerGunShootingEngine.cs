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
            void NewFunction()
            {
                var playerEntities = entitiesDB.QueryEntities<PlayerInputDataStruct>(ECSGroups.Player, out var count);
                var gunEntities =
                    entitiesDB.QueryEntities<GunAttributesEntityStruct, GunEntityViewStruct>(ECSGroups.PlayerGun, out _);

                for (var i = 0; i < count; i++)
                {
                    ref var playerGunComponent = ref gunEntities.Item1[i];
                    playerGunComponent.timer += _time.deltaTime;

                    if (playerEntities[i].fire && playerGunComponent.timer >= playerGunComponent.timeBetweenBullets)
                        Shoot(ref playerGunComponent, gunEntities.Item2[i]);
                }
            }

            while (true)
            {
                ///pay attention: the code of this example started in a naive way, it originally assumed that
                /// there was just one player available, which resulting code could have promoted some not
                /// very good practices. However assuming that there is a relationship 1:1 between entities
                /// between groups is just naive. This works because there is just one player
                NewFunction();

                yield return null;
            }
        }

        /// <summary>
        ///     Design note: shooting and find a target are possibly two different responsibilities
        ///     and probably would need two different engines.
        /// </summary>
        /// <param name="playerGunEntityView"></param>
        void Shoot(ref GunAttributesEntityStruct playerGunEntityView, GunEntityViewStruct gunFxComponent)
        {
            playerGunEntityView.timer = 0;

            var shootRay = gunFxComponent.gunFXComponent.shootRay;
            var entityHit = _rayCaster.CheckHit(shootRay, playerGunEntityView.range,
                                                GAME_LAYERS.ENEMY_LAYER,
                                                GAME_LAYERS.SHOOTABLE_MASK | GAME_LAYERS.ENEMY_MASK, out var point,
                                                out var instanceID);

            if (entityHit)
            {
                var damageInfo = new DamageInfo(playerGunEntityView.damagePerShot, point);

                //note how the GameObject GetInstanceID is used to identify the entity as well
                if (instanceID != -1)
                    entitiesDB.QueryEntity<DamageableEntityStruct>((uint) instanceID, ECSGroups.PlayerTargets)
                              .damageInfo = damageInfo;

                playerGunEntityView.lastTargetPosition = point;
            }
            else
                playerGunEntityView.lastTargetPosition = shootRay.origin + shootRay.direction * playerGunEntityView.range;
            
            
            entitiesDB.PublishEntityChange<GunAttributesEntityStruct>(gunFxComponent.ID);
        }
    }
}