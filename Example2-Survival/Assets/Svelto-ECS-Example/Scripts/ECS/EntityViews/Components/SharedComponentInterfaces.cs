using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public interface IAnimationComponent: IComponent
    {
        string playAnimation { set; get; }
        AnimationState animationState { set; }
        bool reset { set; }
    }

    public interface IPositionComponent: IComponent
    {
        Vector3 position { get; }
    }

    public interface ITransformComponent: IPositionComponent
    {
        new Vector3 position { set; }
        Quaternion rotation { set; }
    }
    
    public interface ILayerComponent
    {
        int layer { set; }
    }

    public interface IRigidBodyComponent: IComponent
    {
        bool isKinematic { set; }
    }

    public interface ISpeedComponent: IComponent
    {
        float movementSpeed { get; }
    }

    public interface IDamageSoundComponent: IComponent
    {
        AudioType playOneShot { set; }
    }
}
