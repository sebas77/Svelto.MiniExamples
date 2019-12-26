using System;
using System.Collections;
using Svelto.Tasks.Enumerators;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public class PlayerGunShootingFXsEngine : IQueryingEntitiesEngine
    {
        public IEntitiesDB        entitiesDB { set; private get; }

        public PlayerGunShootingFXsEngine(IEntityStreamConsumerFactory factory) { _factory = factory; }

        public void Ready()
        {
            PlayerHasShot().Run();
        }

        IEnumerator PlayerHasShot()
        {
            void IterateGunFires(Consumer<GunAttributesEntityStruct> consumer)
            {
                while (consumer.TryDequeue(out var gunEntityStruct, out var egid))
                {
                    ref var gunFXComponent = ref entitiesDB.QueryEntity<GunEntityViewStruct>(egid).gunFXComponent;

                    ref var playerGunEntityView = ref gunEntityStruct;

                    // Play the gun shot audioclip.
                    gunFXComponent.playAudio = true;

                    // Enable the light.
                    gunFXComponent.lightEnabled = true;

                    // Stop the particles from playing if they were, then start the particles.
                    gunFXComponent.play = false;
                    gunFXComponent.play = true;

                    ref var gunComponent = ref gunEntityStruct;
                    var     shootRay     = gunFXComponent.shootRay;

                    // Enable the line renderer and set it's first position to be the end of the gun.
                    gunFXComponent.lineEnabled       = true;
                    gunFXComponent.lineStartPosition = shootRay.origin;

                    gunFXComponent.lineEndPosition = gunComponent.lastTargetPosition;

                    //Note:
                    //this is going to allocate. With Svelto Tasks 1.5 it's not simple to find a workaround for this.
                    //There are some tricks, but they are out of the scope of this example. Svelto Tasks 2.0 allows
                    //simpler solution, like using IEnumerator as structs.
                    DisableFXAfterTime(playerGunEntityView.timeBetweenBullets * gunFXComponent.effectsDisplayTime).Run();
                }
            }

            var generateConsumer = _factory.GenerateConsumer<GunAttributesEntityStruct>("GunFireConsumer", 1);

            while (true)
            {
                IterateGunFires(generateConsumer);

                yield return null;
            }
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
        
        IEntityStreamConsumerFactory _factory;
    }
}