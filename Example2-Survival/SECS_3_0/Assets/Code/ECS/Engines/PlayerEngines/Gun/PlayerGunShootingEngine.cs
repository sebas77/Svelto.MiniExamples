using System.Collections;
using Svelto.Common;
using Svelto.ECS.Extensions;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    [Sequenced(nameof(EnginesEnum.PlayerGunShootingEngine))]
    public class PlayerGunShootingEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public PlayerGunShootingEngine(IRayCaster rayCaster, ITime time)
        {
            _rayCaster = rayCaster;
            _time      = time;
            _shootTick = Tick();
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() { }

        public void Step() { _shootTick.MoveNext(); }
        public string name => nameof(PlayerGunShootingEngine);

        IEnumerator Tick()
        {
            void Shot()
            {
                var (guns, gunsViews, count) =
                    entitiesDB.QueryEntities<GunAttributesComponent, GunEntityViewComponent>(
                        ECSGroups.PlayersGunsGroup);

                for (var i = 0; i < count; i++)
                {
                    ref var playerGunComponent = ref guns[i];
                    playerGunComponent.timer += _time.deltaTime;

                    //The Players and the Guns are not directly linked. However the guns are created with the playerID
                    //so we can query the player from the gun, and viceversa, through the ID
                    //QueryEntity and EGIDMAppers are the only safe way to link entities between each other
                    //(even for hierarchies for example). These methods will be optimised even further in future
                    ref var playerInput = ref entitiesDB.QueryEntity<PlayerInputDataComponent>(
                        new EGID(playerGunComponent.ID.entityID, ECSGroups.PlayersGroup));

                    if (playerInput.fire && playerGunComponent.timer >= playerGunComponent.timeBetweenBullets)
                        Shoot(ref playerGunComponent, gunsViews[i]);
                }
            }

            while (true)
            {
                ///pay attention: the code of this example started in a naive way, it originally assumed that
                /// there was just one player available, which resulting code could have promoted some not
                /// very good practices. However assuming that there is a relationship 1:1 between entities
                /// between groups is just naive. This works because there is just one player
                Shot();

                yield return null;
            }
        }

        /// <summary>
        ///     Design note: shooting and find a target are possibly two different responsibilities
        ///     and probably would need two different engines. This is a simple project and doesn't need
        ///     this level of abstraction. Never abstract too early! Early abstraction is the root of all the evil!
        ///     Svelto and ECS help immensely to abstract only when it's actually needed
        /// </summary>
        /// <param name="playerGunEntityView"></param>
        void Shoot(ref GunAttributesComponent playerGunEntityView, GunEntityViewComponent gunFxComponent)
        {
            playerGunEntityView.timer = 0;

            var shootRay = gunFxComponent.gunFXComponent.shootRay;
            var entityHit = _rayCaster.CheckHit(shootRay, playerGunEntityView.range, GAME_LAYERS.ENEMY_LAYER
                                              , GAME_LAYERS.SHOOTABLE_MASK | GAME_LAYERS.ENEMY_MASK, out var point
                                              , out var instanceID);

            if (entityHit)
            {
                var damageInfo = new DamageInfo(playerGunEntityView.damagePerShot, point);

                //note how the GameObject GetInstanceID is used to identify the entity as well
                if (instanceID != default)
                {
                    entitiesDB.QueryEntity<DamageableComponent>((uint) instanceID, ECSGroups.PlayerTargetsGroup)
                              .damageInfo = damageInfo;
                    
                    entitiesDB.PublishEntityChange<DamageableComponent>(instanceID);
                }

                playerGunEntityView.lastTargetPosition = point;
            }
            else
                playerGunEntityView.lastTargetPosition =
                    shootRay.origin + shootRay.direction * playerGunEntityView.range;

            //
            // PublishEntityChange is the perfect ECS model to communicate event based data change.
            // For each publisher there must be one or more consumers.
            //
            entitiesDB.PublishEntityChange<GunAttributesComponent>(gunFxComponent.ID);
        }

        readonly IRayCaster  _rayCaster;
        readonly ITime       _time;
        readonly IEnumerator _shootTick;
    }
}