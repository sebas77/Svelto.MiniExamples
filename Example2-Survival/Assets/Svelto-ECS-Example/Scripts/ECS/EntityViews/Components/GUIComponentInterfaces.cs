using UnityEngine;

namespace Svelto.ECS.Example.Survive.HUD
{
    public interface IDamageHUDComponent: IComponent
    {
        float speed { get; }
        Color flashColor { get; }
        Color imageColor { set; get;  }
    }

    public interface IHealthSliderComponent: IComponent
    {
        int value { set; }
    }

    public interface IScoreComponent: IComponent
    {
        int score { set; get; }
    }
}
