using System.Collections;
using Svelto.Common;
using Svelto.ECS.Example.Survive.Damage;
using Svelto.ECS.Example.Survive.OOPLayer;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Player.Gun
{
    [Sequenced(nameof(PlayerGunEnginesNames.PlayerFiresGunEngine))]
    public class PlayerFiresGunEngine: IQueryingEntitiesEngine, IStepEngine
    {
        public PlayerFiresGunEngine(IRayCaster rayCaster, ITime time, int shootableLayer, int environmentMask,
            int enemyMask)
        {
            _rayCaster = rayCaster;
            _time = time;
            _shootTick = Tick();
            _shootableLayer = shootableLayer;
            _environmentMask = environmentMask;
            _enemyMask = enemyMask;
        }

        public EntitiesDB entitiesDB { set; private get; }

        public void Ready() { }

        public void Step() => _shootTick.MoveNext();

        public string name => nameof(PlayerFiresGunEngine);

        /// <summary>
        ///     Design note: shooting and find a target are possibly two different responsibilities
        ///     and probably would need two different engines. This is a simple project and doesn't need
        ///     this level of abstraction. Never abstract too early! Early abstraction is the root of all the evil!
        ///     Svelto and ECS help immensely to abstract only when it's actually needed
        /// </summary>
        /// <param name="gunComponent"></param>
        /// <param name="gunOopFxComponent"></param>
        void Shoot(ref GunComponent gunComponent, ref GunOOPEntityComponent gunOopFxComponent)
        {
            Ray shootRay = gunOopFxComponent.shootRay;

            //CheckHit returns the EGID of the entity linked to the gameobject hit
            var hit = _rayCaster.CheckHit(
                shootRay, gunComponent.range, _shootableLayer, _environmentMask, _enemyMask,
                out var point, out var referenceID);

            //invalid entity reference is a valid return from CheckHit, it means that something has been hit but it's not an entity
            if (hit)
            {
                if (referenceID != EntityReference.Invalid)
                {
                    var damageInfo = new DamageInfo(gunComponent.damagePerShot, point);

                    var instanceID = referenceID.ToEGID(entitiesDB);
                    {
                        entitiesDB.QueryEntity<DamageableComponent>(instanceID).damageInfo = damageInfo;
                    }
                }

                gunOopFxComponent.lineEndPosition = point;
            }
            else
                gunOopFxComponent.lineEndPosition = shootRay.origin + shootRay.direction * gunComponent.range;
        }

        IEnumerator Tick()
        {
            while (true)
            {
                var (weapons, gunOOPcomponent, count) = entitiesDB
                       .QueryEntities<GunComponent, GunOOPEntityComponent>(PlayerGun.Gun.Group);

                for (int i = count - 1; i >= 0; i--)
                {
                    weapons[i].timer += _time.deltaTime;

                    if (weapons[i].fired && weapons[i].timer >= weapons[i].timeBetweenBullets)
                    {
                        weapons[i].timer = 0;
                        
                        Shoot(ref weapons[i], ref gunOOPcomponent[i]);
                        gunOOPcomponent[i].effectsEnabledForTime = weapons[i].timeBetweenBullets * 0.2f;
                    }
                }

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
        readonly ITime _time;

        readonly int _shootableLayer;
        readonly int _environmentMask;
        readonly int _enemyMask;
    }
}