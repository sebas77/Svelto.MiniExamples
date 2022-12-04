namespace Svelto.ECS.Example.Survive
{
    public interface IAnimationComponent
    {
        string              playAnimation  { set; get; }
        AnimationState      animationState { set; }
        bool                reset          { set; }
    }
}