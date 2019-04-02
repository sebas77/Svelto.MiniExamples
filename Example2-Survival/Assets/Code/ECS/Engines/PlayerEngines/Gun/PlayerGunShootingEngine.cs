using System.Collections;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public class PlayerGunShootingEngine
        : MultiEntitiesEngine<GunEntityViewStruct, PlayerEntityViewStruct>, IQueryingEntitiesEngine
    {
        readonly IRayCaster                _rayCaster;
        readonly ITaskRoutine<IEnumerator> _taskRoutine;
        readonly ITime                     _time;

        public PlayerGunShootingEngine(IRayCaster rayCaster, ITime time)
        {
            _rayCaster   = rayCaster;
            _time        = time;
            _taskRoutine = TaskRunner.Instance.AllocateNewTaskRoutine(StandardSchedulers.physicScheduler);
            _taskRoutine.SetEnumerator(Tick());
        }

        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready() { _taskRoutine.Start(); }

        protected override void Add(ref GunEntityViewStruct              entityView,
                                    ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        {
        }

        protected override void Remove(ref GunEntityViewStruct entityView, bool itsaSwap) { _taskRoutine.Stop(); }

        protected override void Add(ref PlayerEntityViewStruct           entityView,
                                    ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        {
        }

        protected override void Remove(ref PlayerEntityViewStruct entityView, bool itsaSwap) { _taskRoutine.Stop(); }

        IEnumerator Tick()
        {
            while (entitiesDB.HasAny<PlayerEntityViewStruct>(ECSGroups.Player) == false ||
                   entitiesDB.HasAny<GunEntityViewStruct>(ECSGroups.Player) == false)
                yield return null; //skip a frame

            //never changes
            var playerGunEntities = entitiesDB.QueryEntities<GunEntityViewStruct>(ECSGroups.Player, out var count);
            //never changes
            var playerEntities = entitiesDB.QueryEntities<PlayerInputDataStruct>(ECSGroups.Player, out count);

            while (true)
            {
                var playerGunComponent = playerGunEntities[0].gunComponent;

                playerGunComponent.timer += _time.deltaTime;

                if (playerEntities[0].fire &&
                    playerGunComponent.timer >= playerGunEntities[0].gunComponent.timeBetweenBullets)
                    Shoot(playerGunEntities[0]);

                yield return null;
            }
        }

        /// <summary>
        ///     Design note: shooting and find a target are possibly two different responsibilities
        ///     and probably would need two different engines.
        /// </summary>
        /// <param name="playerGunEntityView"></param>
        void Shoot(GunEntityViewStruct playerGunEntityView)
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