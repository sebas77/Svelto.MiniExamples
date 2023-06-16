using UnityEngine;

namespace Svelto.ECS.Example.Survive.Enemies
{
    public static class EnemyAnimations
    {
        public static readonly int Die =  Animator.StringToHash("Dead");
        public static readonly int TargetDead =  Animator.StringToHash("PlayerDead");
    }
}