namespace Svelto.ECS.Example.Survive
{
    public struct AnimationState
    {
        public readonly int animationID;
        public readonly bool       state;
        
        public AnimationState(int id, bool animState = true)
        {
            animationID = id;
            state = animState;
        }
    }

}