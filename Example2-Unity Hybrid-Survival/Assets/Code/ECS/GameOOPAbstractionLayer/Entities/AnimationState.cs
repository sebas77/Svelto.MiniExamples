using System;

namespace Svelto.ECS.Example.Survive
{
    public struct AnimationState: IEquatable<AnimationState>
    {
        public readonly int animationID;
        public readonly bool state;
        internal bool hasBeenSet; 

        public AnimationState(int id, bool animState = true)
        {
            animationID = id;
            state = animState;
            hasBeenSet = true;
        }

        public bool Equals(AnimationState other)
        {
            return animationID == other.animationID && state == other.state;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(animationID, state);
        }
    }
}