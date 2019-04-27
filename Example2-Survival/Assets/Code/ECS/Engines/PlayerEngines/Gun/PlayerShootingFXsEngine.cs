using System.Collections;
using Svelto.Tasks;
using Svelto.Tasks.Enumerators;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public class PlayerGunShootingFXsEngine : IReactOnAddAndRemove<GunEntityViewStruct>, IQueryingEntitiesEngine
    {
        ITaskRoutine<IEnumerator> _taskRoutine;
        WaitForSecondsEnumerator  _waitForSeconds;
        public IEntitiesDB        entitiesDB { set; private get; }

        public void Ready()
        {
            //In this case a taskroutine is used because we want to have control over when it starts
            //and we want to reuse it.
            _taskRoutine = TaskRunner.Instance.AllocateNewTaskRoutine();
            _taskRoutine.SetEnumeratorProvider(DisableFXAfterTime);
        }

        /// <summary>
        ///     Using the Add/Remove method to hold a local reference of an entity
        ///     is not necessary. Do it only if you find convenient, otherwise
        ///     querying is always cleaner.
        /// </summary>
        /// <param name="playerGunEntityView"></param>
        public void Add(ref GunEntityViewStruct              playerGunEntityView)
        {
            playerGunEntityView.gunHitTargetComponent.targetHit = new DispatchOnSet<bool>(playerGunEntityView.ID);
            playerGunEntityView.gunHitTargetComponent.targetHit.NotifyOnValueSet(PlayerHasShot);

            _waitForSeconds = new WaitForSecondsEnumerator(playerGunEntityView.gunComponent.timeBetweenBullets *
                                                           playerGunEntityView.gunFXComponent.effectsDisplayTime);
        }

        public void Remove(ref GunEntityViewStruct entityView) { }

        void PlayerHasShot(EGID egid, bool targetHasBeenHit)
        {
            var structs = entitiesDB.QueryEntitiesAndIndex<GunEntityViewStruct>(egid, out var index);

            ref var gunFXComponent = ref structs[index].gunFXComponent;

            // Play the gun shot audioclip.
            gunFXComponent.playAudio = true;

            // Enable the light.
            gunFXComponent.lightEnabled = true;

            // Stop the particles from playing if they were, then start the particles.
            gunFXComponent.play = false;
            gunFXComponent.play = true;

            var gunComponent = structs[index].gunComponent;
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

            _taskRoutine.Start();
        }

        IEnumerator DisableFXAfterTime()
        {
            yield return _waitForSeconds;
            // ... disable the effects.
            DisableEffects();
        }

        void DisableEffects()
        {
            var gunEntityViews = entitiesDB.QueryEntities<GunEntityViewStruct>(ECSGroups.Player, out _);

            var fxComponent = gunEntityViews[0].gunFXComponent;
            // Disable the line renderer and the light.
            fxComponent.lineEnabled  = false;
            fxComponent.lightEnabled = false;
            fxComponent.play         = false;
        }
    }
}