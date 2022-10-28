namespace Svelto.ECS.Example.Survive
{
    public struct AnimationState
    {
        public readonly AnimationID animationID;
        public readonly bool       state;
        
        public AnimationState(AnimationID id, bool animState = true)
        {
            animationID = id;
            state = animState;
        }
    }

    public struct AnimationID
    {
        public int id { get; set; }
    }
}