using Svelto.ECS.Unity;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Implementors
{
    public class AnimatorImplementor : MonoBehaviour, IImplementor, IAnimationComponent
    {
        Animator _anim;

        string _animName;

        public string playAnimation
        {
            get { return _animName; }
            set
            {
                _animName = value;
                _anim.SetTrigger(value);
            }
        }

        public AnimationState animationState { set { _anim.SetBool(value.name, value.state); } }

        public bool reset
        {
            set
            {
                if (value) _anim.Rebind();
                _animName = string.Empty;
            }
        }

        void Awake() { _anim = GetComponent<Animator>(); }
    }
}