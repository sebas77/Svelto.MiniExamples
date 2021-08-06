using System.Collections;
using Svelto.Common;
using Svelto.ECS.Example.Survive.Wave;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.HUD
{
    [Sequenced(nameof(EnginesNames.UpdateWaveHUDEngine))]
    public class UpdateWaveHUDEngine : IQueryingEntitiesEngine, IStepEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
            _updateWaveHUD = UpdateWaveHUD();
        }
        
        public void   Step() { _updateWaveHUD.MoveNext(); }
        public string name   => nameof(UpdateWaveHUDEngine);

        IEnumerator UpdateWaveHUD()
        {
            while (entitiesDB.HasAny<HUDEntityViewComponent>(ECSGroups.GUICanvas) == false
                || entitiesDB.HasAny<WaveComponent>(ECSGroups.Waves) == false)
                {yield return null;}
            
            var hudEntityView = entitiesDB.QueryUniqueEntity<HUDEntityViewComponent>(ECSGroups.GUICanvas);

            var consumer = _consumerFactory.GenerateConsumer<WaveComponent>("WaveConsumer1", 1);

            
            while (true)
            {
                while (consumer.TryDequeue(out var waveEntity, out var egid))
                {
                    hudEntityView.waveProgressionComponent.enemiesLeft = waveEntity.enemiesLeft;
                    if (hudEntityView.waveComponent.wave != waveEntity.wave)
                    {
                        hudEntityView.waveComponent.wave = waveEntity.wave;
                        hudEntityView.waveComponent.showHUD = true;
                        var waitForSecondsEnumerator = new WaitForSecondsEnumerator(1.5f);
                        while (waitForSecondsEnumerator.MoveNext())
                            yield return null;
                        hudEntityView.waveComponent.showHUD = false;
                    }
                }

                yield return null;
            }
        }

        public UpdateWaveHUDEngine(IEntityStreamConsumerFactory consumerFactory) { _consumerFactory = consumerFactory; }
        
        IEntityStreamConsumerFactory          _consumerFactory;
        IEnumerator                           _updateWaveHUD;
    }
}