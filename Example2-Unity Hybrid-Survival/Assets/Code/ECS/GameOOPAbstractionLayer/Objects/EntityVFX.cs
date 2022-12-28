using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class EntityVFX : MonoBehaviour
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
        public void play(in Vector3 position)
        {
            particle.transform.position = position;
            particle.Play();
        }

        void Awake() { particle = GetComponentInChildren<ParticleSystem>(); }
    }
}