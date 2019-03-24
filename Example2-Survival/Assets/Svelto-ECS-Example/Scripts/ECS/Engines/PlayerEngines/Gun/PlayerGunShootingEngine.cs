using System.Collections;
using UnityEngine;
using Svelto.Tasks;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public class PlayerGunShootingEngine : MultiEntitiesEngine<GunEntityViewStruct, PlayerEntityViewStruct>, 
        IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready()
        {
            _taskRoutine.Start();
        }
        
        public PlayerGunShootingEngine(IRayCaster rayCaster, ITime time)
        {
            _rayCaster             = rayCaster;
            _time                  = time;
            _taskRoutine           = TaskRunner.Instance.AllocateNewTaskRoutine(StandardSchedulers.physicScheduler);
            _taskRoutine.SetEnumerator(Tick());
        }

        protected override void Add(ref GunEntityViewStruct entityView)
        {}

        protected override void Remove(ref GunEntityViewStruct entityView)
        {
            _taskRoutine.Stop();
        }

        protected override void Add(ref PlayerEntityViewStruct entityView)
        {}

        protected override void Remove(ref PlayerEntityViewStruct entityView)
        {
            _taskRoutine.Stop();
        }

        IEnumerator Tick()
        {
            while (entitiesDB.HasAny<PlayerEntityViewStruct>(ECSGroups.Player) == false ||
                   entitiesDB.HasAny<GunEntityViewStruct>(ECSGroups.Player) == false)
            {
                yield return null; //skip a frame
            }

            int count;
            //never changes
            var playerGunEntities = entitiesDB.QueryEntities<GunEntityViewStruct>(ECSGroups.Player, out count);
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
        /// Design note: shooting and find a target are possibly two different responsibilities
        /// and probably would need two different engines. 
        /// </summary>
        /// <param name="playerGunEntityView"></param>
        void Shoot(GunEntityViewStruct playerGunEntityView)
        {
            var playerGunComponent    = playerGunEntityView.gunComponent;
            var playerGunHitComponent = playerGunEntityView.gunHitTargetComponent;

            playerGunComponent.timer = 0;

            Vector3 point;
            int instanceID;
            var entityHit = _rayCaster.CheckHit(playerGunComponent.shootRay,
                                                playerGunComponent.range,
                                                GAME_LAYERS.ENEMY_LAYER,
                                                GAME_LAYERS.SHOOTABLE_MASK | GAME_LAYERS.ENEMY_MASK,
                                                out point, out instanceID);
            
            if (entityHit)
            {
                var damageInfo =
                    new
                        DamageInfo(playerGunComponent.damagePerShot,
                                   point);
                
                //note how the GameObject GetInstanceID is used to identify the entity as well
                if (instanceID != -1)
                    entitiesDB.ExecuteOnEntity(instanceID, ECSGroups.PlayerTargets, ref damageInfo,
                                               (ref DamageableEntityStruct entity, ref DamageInfo info) => //
                                               { //never catch external variables so that the lambda doesn't allocate
                                                   entity.damageInfo = info;
                                               });

                playerGunComponent.lastTargetPosition = point;
                playerGunHitComponent.targetHit.value = true;
            }
            else
                playerGunHitComponent.targetHit.value = false;
        }

        readonly IRayCaster            _rayCaster;
        readonly ITime                 _time;
        readonly ITaskRoutine<IEnumerator>          _taskRoutine;
    }
}