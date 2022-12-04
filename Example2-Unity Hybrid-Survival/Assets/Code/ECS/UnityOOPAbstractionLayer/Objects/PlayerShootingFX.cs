using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public sealed class PlayerShootingFX: MonoBehaviour
    {
        internal Ray shootRay => new Ray(_transform.position, _transform.forward);
        public float   effectsDisplayTime { get; } = 0.2f;

        public void PlayEffects(Vector3 lineEndPosition)
        {
            _gunParticles.Play();
            _gunLight.enabled = true;
            _gunLine.enabled = true;
            _gunLine.SetPosition(0, shootRay.origin);
            _gunLine.SetPosition(1, lineEndPosition);
            _gunAudio.Play();
        }
        
        public void StopEffects()
        {
            _gunParticles.Stop();
            _gunLight.enabled = false;
            _gunLine.enabled = false;
        }
        
        void Awake()
        {
            _transform = transform;

            // Set up the references.
            _gunParticles = GetComponent<ParticleSystem>();
            _gunLine = GetComponent<LineRenderer>();
            _gunAudio = GetComponent<AudioSource>();
            _gunLight = GetComponent<Light>();
        }

        AudioSource _gunAudio;        // Reference to the audio source.
        Light _gunLight;              // Reference to the light component.
        LineRenderer _gunLine;        // Reference to the line renderer.
        ParticleSystem _gunParticles; // Reference to the particle system.

        Transform _transform;
    }
}