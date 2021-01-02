using Svelto.ECS.Example.Survive.HUD;
using Svelto.ECS.Hybrid;
using UnityEngine;
using UnityEngine.UI;

namespace Svelto.ECS.Example.Survive.Implementors.HUD
{
    public class ScoreManagerImplementor : MonoBehaviour, IImplementor, IScoreComponent
    {
        public int score
        {
            get => _score;
            set
            {
                _score     = value;
                _text.text = "score: " + _score;
            }
        }

        void Awake()
        {
            // Set up the reference.
            _text = GetComponent<Text>();

            // Reset the score.
            _score = 0;
        }

        int  _score;
        Text _text;
    }
}