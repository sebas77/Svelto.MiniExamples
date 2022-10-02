using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public interface IAnimationComponent
    {
        string              playAnimation  { set; get; }
        AnimationState      animationState { set; }
        bool                reset          { set; }
    }

    public interface IPositionComponent
    {
        Vector3 position { get; }
    }

    public interface ITransformComponent : IPositionComponent
    {
        new Vector3 position { set; }
        Quaternion  rotation { set; }
    }

    public interface ILayerComponent
    {
        int layer { set; }
    }

    public interface IRigidBodyComponent
    {
        bool    isKinematic { set; }
        Vector3 velocity    { set; }
    }

    public interface IDamageSoundComponent
    {
        AudioType playOneShot { set; }
    }
}