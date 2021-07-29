using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.UI;

namespace Svelto.ECS.Example.Survive.Implementors.HUD
{
    public class WaveProgressManagerImplementor : MonoBehaviour, IImplementor, IWaveProgressionComponent
    {
        public int enemiesLeft
        {
            get => _enemiesLeft;
            set
            {
                _enemiesLeft = value;
                _text.text = _enemiesLeft.ToString();
            }
        }

        void Awake()
        {
            // Set up the reference.
            _text = GetComponent<Text>();

            // Reset the score.
            _enemiesLeft = 0;
        }

        int  _enemiesLeft;
        Text _text;
    }
}