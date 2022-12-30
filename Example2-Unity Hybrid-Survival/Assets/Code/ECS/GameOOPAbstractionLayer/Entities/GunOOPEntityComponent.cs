using System;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public struct GunOOPEntityComponent : IEntityComponent
    {
        public Ray shootRay { get; internal set; }

        public Vector3 lineEndPosition;
        
        internal bool effectsEnabled
        {
            set
            {
                _play = value;
                _playChanged = true;
            }
        }

        internal PlayState GetStateAndReset()
        {
            var playChanged = _playChanged;
            _playChanged = false;
            switch (playChanged)
            {
                case false when _play == true:
                    return PlayState.play;
                case true when _play == true:
                    return PlayState.start;
                case true when _play == false:
                    return PlayState.stop;
                default:
                    return PlayState.inert;
            }
        }

        bool _play;
        bool _playChanged;
        float _effectsEnabledForTime;

        public float effectsEnabledForTime
        {
            set
            {
                if (_effectsEnabledForTime <= 0)
                    effectsEnabled = true;
                
                _effectsEnabledForTime = value;
            }
            internal readonly get { return _effectsEnabledForTime; }
        }
    }

    enum PlayState
    {
        start,
        play,
        stop,
        inert
    }
}