using Svelto.ECS.Example.Survive.Characters.Player.Gun;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public class PlayerShootingFXImplementor : MonoBehaviour, IImplementor, IGunFXComponent
    {
        public Ray shootRay => new Ray(_transform.position, _transform.forward);

        public float   effectsDisplayTime { get; } = 0.2f;
        public Vector3 lineEndPosition    { set => _gunLine.SetPosition(1, value); }
        public Vector3 lineStartPosition  { set => _gunLine.SetPosition(0, value); }
        public bool    lineEnabled        { set => _gunLine.enabled = value; }

        public bool play
        {
            set
            {
                if (value)
                    _gunParticles.Play();
                else
                    _gunParticles.Stop();
            }
        }

        public bool lightEnabled { set => _gunLight.enabled = value; }

        public bool playAudio
        {
            set
            {
                if (value)
                    _gunAudio.Play();
                else
                    _gunAudio.Stop();
            }
        }

        void Awake()
        {
            _transform = transform;

            // Set up the references.
            _gunParticles = GetComponent<ParticleSystem>();
            _gunLine      = GetComponent<LineRenderer>();
            _gunAudio     = GetComponent<AudioSource>();
            _gunLight     = GetComponent<Light>();
        }

        AudioSource    _gunAudio; // Reference to the audio source.
        Light          _gunLight; // Reference to the light component.
        LineRenderer   _gunLine; // Reference to the line renderer.
        ParticleSystem _gunParticles; // Reference to the particle system.

        Transform _transform;
    }
}