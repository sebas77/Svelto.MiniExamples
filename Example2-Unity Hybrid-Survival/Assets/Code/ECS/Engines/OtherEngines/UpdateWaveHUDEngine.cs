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
            
            while (true)
            {
                var waitForSecondsEnumerator = new WaitForSecondsEnumerator(0.2f);
                while (waitForSecondsEnumerator.MoveNext())
                    yield return null;

                var waveEntity = entitiesDB.QueryUniqueEntity<WaveComponent>(ECSGroups.Waves);
                if (waveEntity.enemiesLeft != hudEntityView.waveProgressionComponent.enemiesLeft)
                    hudEntityView.waveProgressionComponent.enemiesLeft = waveEntity.enemiesLeft;


                yield return null;
            }
        }

        public UpdateWaveHUDEngine() {}
        
        IEnumerator                           _updateWaveHUD;
    }
}