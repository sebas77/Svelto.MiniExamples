using System;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Implementors
{
    public class AudioImplementor : MonoBehaviour, IImplementor, 
        IDamageSoundComponent
    {
        public AudioClip deathClip;                 // The sound to play when the enemy dies.
        public AudioClip damageClip;                 // The sound to play when the enemy dies.

        void Awake ()
        {// Setting up the references.
            _audioSource = GetComponent <AudioSource>();
        }
        
        public AudioType playOneShot
        {
            set
            {
                switch (value)
                {
                    case AudioType.damage:
                        _audioSource.PlayOneShot(damageClip);
                        break;
                    case AudioType.death:
                        _audioSource.PlayOneShot(deathClip);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("value", value, null);
                }
            }
        }

        AudioSource     _audioSource;           // Reference to the audio source.
    }
}
