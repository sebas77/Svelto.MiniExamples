using System.Collections;
using Svelto.ECS.Example.Survive.OOPLayer;

namespace Svelto.ECS.Example.Survive.Player.Gun
{
    public class PlayerGunShootingFXsEngine: IQueryingEntitiesEngine, IStepEngine
    {
        public EntitiesDB entitiesDB { set; private get; }

        public PlayerGunShootingFXsEngine(IEntityStreamConsumerFactory factory)
        {
            _factory = factory;
        }

        public void Ready()
        {
            _playerHasShot = PlayerHasShot();
        }

        public void Step()
        {
            _playerHasShot.MoveNext();
        }

        public string name => nameof(PlayerGunShootingFXsEngine);

        IEnumerator PlayerHasShot()
        {
            void SetValues(GunComponent gunComponent, out float waitTime,
                ref GunOOPEntityComponent gunOopFXComponent)
            {
                gunOopFXComponent.effectsEnabled = true;
                gunOopFXComponent.lineEndPosition = gunComponent.lastTargetPosition;

                waitTime = gunComponent.timeBetweenBullets * gunOopFXComponent.effectsDisplayTime;
            }
            
            void DisableEffects(ref GunOOPEntityComponent gunOopFXComponent)
            {
                // Disable the line renderer and the light.
                gunOopFXComponent.effectsEnabled = false;
            }

            var consumer = _factory.GenerateConsumer<GunComponent>("GunFireConsumer", 1);
            WaitForSecondsEnumerator waitForSecondsEnumerator = default;

            while (true)
            {
                //Consume the entity change sent from PlayerGunShootingEngine
                while (consumer.TryDequeue(out var gunComponent, out var egid))
                {
                    SetValues(gunComponent, out var wait, ref entitiesDB.QueryEntity<GunOOPEntityComponent>(egid));

                    waitForSecondsEnumerator.Reset(wait);
                    while (waitForSecondsEnumerator.MoveNext())
                        yield return null;
                    // ... disable the effects.
                    DisableEffects(ref entitiesDB.QueryEntity<GunOOPEntityComponent>(egid));
                }

                yield return null;
            }
        }

        readonly IEntityStreamConsumerFactory _factory;
        IEnumerator _playerHasShot;
    }
}