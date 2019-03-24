namespace Svelto.ECS.Example.Survive
{
    public struct AnimationState
    {
        public readonly string name;
        public readonly bool   state;

        public AnimationState(string animName, bool animState)
        {
            name  = animName;
            state = animState;
        }
    }
}