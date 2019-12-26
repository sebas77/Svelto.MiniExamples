using System;
using System.Collections;
using Svelto.Tasks.Enumerators;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public class PlayerGunShootingFXsEngine : IReactOnAddAndRemove<GunEntityViewStruct>, IQueryingEntitiesEngine
    {
        public PlayerGunShootingFXsEngine() { _playerHasShot = PlayerHasShot; }
        public IEntitiesDB        entitiesDB { set; private get; }
        public void Ready() {  }

        public void Add(ref GunEntityViewStruct playerGunEntityView, EGID egid)
        {
            playerGunEntityView.gunHitTargetComponent.targetHit = new DispatchOnSet<bool>(egid);
            playerGunEntityView.gunHitTargetComponent.targetHit.NotifyOnValueSet(_playerHasShot);
        }

        public void Remove(ref GunEntityViewStruct playerGunEntityView, EGID egid) 
        {
            playerGunEntityView.gunHitTargetComponent.targetHit.StopNotify(_playerHasShot); 
            playerGunEntityView.gunHitTargetComponent.targetHit = null;
        }

        void PlayerHasShot(EGID egid, bool targetHasBeenHit)
        {
            var structs = entitiesDB.QueryEntitiesAndIndex<GunEntityViewStruct>(egid, out var index);

            ref var playerGunEntityView = ref structs[index].gunComponent ;
            ref var gunFXComponent = ref structs[index].gunFXComponent;

            // Play the gun shot audioclip.
            gunFXComponent.playAudio = true;

            // Enable the light.
            gunFXComponent.lightEnabled = true;

            // Stop the particles from playing if they were, then start the particles.
            gunFXComponent.play = false;
            gunFXComponent.play = true;

            ref var gunComponent = ref structs[index].gunComponent;
            var shootRay     = gunComponent.shootRay;

            // Enable the line renderer and set it's first position to be the end of the gun.
            gunFXComponent.lineEnabled       = true;
            gunFXComponent.lineStartPosition = shootRay.origin;

            // Perform the raycast against gameobjects on the shootable layer and if it hits something...
            if (targetHasBeenHit)
                gunFXComponent.lineEndPosition = gunComponent.lastTargetPosition;
            // If the raycast didn't hit anything on the shootable layer...
            else
                gunFXComponent.lineEndPosition = shootRay.origin + shootRay.direction * gunComponent.range;

            //Note:
            //this is going to allocate. With Svelto Tasks 1.5 it's not simple to find a workaround for this.
            //There are some tricks, but they are out of the scope of this example. Svelto Tasks 2.0 allows
            //simpler solution, like using IEnumerator as structs.
            DisableFXAfterTime(playerGunEntityView.timeBetweenBullets * gunFXComponent.effectsDisplayTime).Run();
        }

        IEnumerator DisableFXAfterTime(float wait)
        {
            yield return new WaitForSecondsEnumerator(wait);
            // ... disable the effects.
            DisableEffects();
        }

        void DisableEffects()
        {
            var gunEntityViews = entitiesDB.QueryEntities<GunEntityViewStruct>(ECSGroups.PlayerGun, out _);

            var fxComponent = gunEntityViews[0].gunFXComponent;
            // Disable the line renderer and the light.
            fxComponent.lineEnabled  = false;
            fxComponent.lightEnabled = false;
            fxComponent.play         = false;
        }
        
        readonly Action<EGID, bool> _playerHasShot;

    }
}