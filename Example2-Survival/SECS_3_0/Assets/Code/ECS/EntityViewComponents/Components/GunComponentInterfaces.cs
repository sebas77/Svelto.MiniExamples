using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Player.Gun
{
    public interface IGunFXComponent
    {
        Ray shootRay { get; }
        float   effectsDisplayTime { get; }
        Vector3 lineEndPosition    { set; }
        Vector3 lineStartPosition  { set; }
        bool    lineEnabled        { set; }
        bool    play               { set; }
        bool    lightEnabled       { set; }
        bool    playAudio          { set; }
    }
}