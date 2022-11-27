using System.Collections;
using Svelto.Common;
using Svelto.ECS.Example.Survive.Damage;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player.Gun
{
    public enum PlayerGunEnginesNames
    {
        PlayerGunShootingEngine
    }

    [Sequenced(nameof(PlayerGunEnginesNames.PlayerGunShootingEngine))]
    public class PlayerFiresGunEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public PlayerFiresGunEngine(IRayCaster rayCaster, ITime time, int shootableLayer, int shootableMask)
        {
            _rayCaster      = rayCaster;
            _time           = time;
            _shootTick      = Tick();
            _shootableLayer = shootableLayer;
            _shootableMask  = shootableMask;
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready()
        {
        }

        public void Step()
        {
            _shootTick.MoveNext();
        }

        public string name => nameof(PlayerFiresGunEngine);

        /// <summary>
        ///     Design note: shooting and find a target are possibly two different responsibilities
        ///     and probably would need two different engines. This is a simple project and doesn't need
        ///     this level of abstraction. Never abstract too early! Early abstraction is the root of all the evil!
        ///     Svelto and ECS help immensely to abstract only when it's actually needed
        /// </summary>
        /// <param name="playerGunEntityView"></param>
        /// <param name="gunFxComponent"></param>
        /// <param name="gunFxComponentID"></param>
        void Shoot(ref GunAttributesComponent playerGunEntityView, in GunEntityComponent gunFxComponent,
            EGID gunFxComponentID)
        {
            playerGunEntityView.timer = 0;

            Ray shootRay = gunFxComponent.shootRay;

            //CheckHit returns the EGID of the entity linked to the gameobject hit
            var hit = _rayCaster.CheckHit(shootRay, playerGunEntityView.range, _shootableLayer, _shootableMask,
                out var point, out var referenceID);

            //invalid entity reference is a valid return from CheckHit, it means that something has been hit but it's not an entity
            if (hit && referenceID != EntityReference.Invalid)
            {
                var damageInfo = new DamageInfo(playerGunEntityView.damagePerShot, point);

                var instanceID = referenceID.ToEGID(entitiesDB);
                {
                    entitiesDB.QueryEntity<DamageableComponent>(instanceID).damageInfo = damageInfo;

                    entitiesDB.PublishEntityChange<DamageableComponent>(instanceID);
                }

                playerGunEntityView.lastTargetPosition = point;
            }
            else
                playerGunEntityView.lastTargetPosition =
                    shootRay.origin + shootRay.direction * playerGunEntityView.range;

            entitiesDB.PublishEntityChange<GunAttributesComponent>(gunFxComponentID);
        }

        IEnumerator Tick()
        {
            void Shoot()
            {
                //todo: this needs to be split. This will just set the gunComponent to fire state
                //the rest of the code will be executed by an engine in the gun layer
                foreach (var ((playersInputs, weapons, count), _) in entitiesDB
                            .QueryEntities<PlayerInputDataComponent, WeaponComponent>(Player.Groups))
                {
                    for (var i = 0; i < count; i++)
                    {
                        var input = playersInputs[i];
                        if (input.fire)
                        {
                            var     gunEGID = weapons[i].weapon.ToEGID(entitiesDB);
                            ref var playerGunComponent = ref entitiesDB.QueryEntity<GunAttributesComponent>(gunEGID);
                            ref var playerVisualGunComponent = ref entitiesDB.QueryEntity<GunEntityComponent>(gunEGID);

                            playerGunComponent.timer += _time.deltaTime;

                            if (playerGunComponent.timer >= playerGunComponent.timeBetweenBullets)
                                this.Shoot(ref playerGunComponent, playerVisualGunComponent, gunEGID);
                        }
                    }
                }
            }

            while (true)
            {
                Shoot();

                yield return null;
            }
        }

        /// <summary>
        /// Pay attention: _raycaster is a stateless object so in reality it just encapsulate logic to not
        /// repeat the same code over and over. Using an object instead than a static class is interesting
        /// because it would be possible to inject different implementations in the engine. 
        /// </summary>
        readonly IRayCaster _rayCaster;

        readonly IEnumerator _shootTick;
        readonly ITime       _time;

        readonly int _shootableLayer;
        readonly int _shootableMask;
    }
}