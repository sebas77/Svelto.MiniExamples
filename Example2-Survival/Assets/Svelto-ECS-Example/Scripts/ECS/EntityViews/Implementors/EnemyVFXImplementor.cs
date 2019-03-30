using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyVFXImplementor : MonoBehaviour, IImplementor, 
        IEnemyVFXComponent
    {
        void Awake ()
        {
            particle = GetComponentInChildren <ParticleSystem>();
        }

        /// <summary>
        ///You may wonder if this is ECS. Svelto.ECS has some unique features to make it 
        ///possible to write a whole game with it. One is to use implementors to wrap
        ///preexisting platform functionalities, like the unity animaton system. 
        ///Without this solution would not be possible to totally wrap pre-existing logic.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        public bool play
        {
            set
            {
                if (value == true)
                {
                    particle.Play();
                }
            }
        }

        public ParticleSystem  particle;         // Reference to the particle system that plays when the enemy is damaged.
        public Vector3 position { set { particle.transform.position = value; } }
    }
}
