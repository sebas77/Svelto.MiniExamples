using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.UI;

namespace Svelto.ECS.Example.Survive.Implementors.HUD
{
    public class WaveImplementor : MonoBehaviour, IImplementor, IWaveComponent
    {
        public int wave 
        {
            get => _wave;
            set
            {
                _wave = value;
                _text.text = "Wave: " + _wave.ToString();
            }
        }

        public bool showHUD
        {
            set
            {
                _showHUD = value;
                _text.enabled = value;
            }
        }

        void Awake()
        {
            // Set up the reference.
            _text = GetComponent<Text>();

            // Reset the score.
            _wave = 0;
        }

        int  _wave;
        bool _showHUD;
        Text _text;
    }
}