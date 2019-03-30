using UnityEngine;

namespace Svelto.ECS.Example.Survive.Implementors
{
    public class AnimatorImplementor : MonoBehaviour, IImplementor, IAnimationComponent
    {
        void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public string playAnimation
        {
            get { return _animName; } set { _animName = value; _anim.SetTrigger(value);} 
        }
        
        public AnimationState animationState {
            set { _anim.SetBool(value.name, value.state);}
        }
        
        public bool reset { set { if (value == true) _anim.Rebind();
            _animName = string.Empty;
        } }
        
        string _animName;
        Animator _anim;
    }
}