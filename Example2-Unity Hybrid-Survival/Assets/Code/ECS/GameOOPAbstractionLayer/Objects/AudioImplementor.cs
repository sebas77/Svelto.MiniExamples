using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class AudioImplementor : MonoBehaviour
    {
        public AudioClip[] clips;

        void Awake()
        {
            // Setting up the references.
            _audioSource = GetComponent<AudioSource>();
        }
        
        public void PlayOneShot(int playOneShot)
        {
            _audioSource.PlayOneShot(clips[playOneShot - 1]);
        }

        AudioSource _audioSource; // Reference to the audio source.
    }
}