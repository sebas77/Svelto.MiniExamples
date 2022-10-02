using Svelto.ECS.Example.Survive.Player;

namespace Svelto.ECS.Example.Survive
{
    public struct AnimationState
    {
        public readonly AnimationID animationID;
        public readonly bool       state;
        public readonly bool istrigger;

        public AnimationState(AnimationID id, bool animState = true)
        {
            animationID = id;
            state = animState;
            istrigger = false;
        }
    }

    public struct AnimationID
    {
    }
}