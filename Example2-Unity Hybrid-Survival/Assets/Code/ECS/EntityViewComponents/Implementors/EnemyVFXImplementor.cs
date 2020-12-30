using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyVFXImplementor : MonoBehaviour, IImplementor, IEnemyVFXComponent
    {
        public ParticleSystem particle; // Reference to the particle system that plays when the enemy is damaged.

        /// <summary>
        ///     You may wonder if this is ECS. Svelto.ECS has some unique features to make it
        ///     possible to write a whole game with it. One is to use implementors to wrap
        ///     preexisting platform functionalities, like the unity animaton system.
        ///     Without this solution would not be possible to totally wrap pre-existing logic.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        public bool play
        {
            set
            {
                if (value) particle.Play();
            }
        }

        public Vector3 position { set => particle.transform.position = value; }

        void Awake() { particle = GetComponentInChildren<ParticleSystem>(); }
    }
}