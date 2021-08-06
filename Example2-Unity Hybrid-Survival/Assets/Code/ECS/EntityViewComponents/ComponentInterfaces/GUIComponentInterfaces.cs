using UnityEngine;

namespace Svelto.ECS.Example.Survive.HUD
{
    public interface IDamageHUDComponent
    {
        float speed      { get; }
        Color flashColor { get; }
        Color imageColor { set; get; }
    }

    public interface IHealthSliderComponent
    {
        int value { set; }
    }

    public interface IScoreComponent
    {
        int score { set; get; }
    }

    public interface IWaveProgressionComponent
    {
        int enemiesLeft { set; get; }
    }

    public interface IWaveComponent
    {
        int wave { set; get; }
        bool showHUD { set; }
    }


    public interface IAmmoComponent
    {
        int ammo {get; set;}
    }
}