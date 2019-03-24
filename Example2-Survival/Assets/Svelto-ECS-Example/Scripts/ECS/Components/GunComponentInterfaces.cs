using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public interface IGunAttributesComponent: IComponent
    {
        float   timeBetweenBullets { get; }
        Ray     shootRay           { get; }
        float   range              { get; }
        int     damagePerShot      { get; }
        float   timer              { get; set; }
        Vector3 lastTargetPosition { get; set; }
    }

    public interface IGunHitTargetComponent : IComponent
    {
        DispatchOnSet<bool> targetHit { get; }
    }

    public interface IGunFXComponent: IComponent
    {
        float   effectsDisplayTime { get; }
        Vector3 lineEndPosition    { set; }
        Vector3 lineStartPosition  { set; }
        bool    lineEnabled        { set; }
        bool    play               { set; }
        bool    lightEnabled       { set; }
        bool    playAudio          { set; }
    }
}