using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters
{
    public static class GAME_LAYERS
    {
        public static readonly int SHOOTABLE_MASK = LayerMask.GetMask("Shootable");
        public static readonly int ENEMY_MASK     = LayerMask.GetMask("Enemies");
        public static readonly int NOT_SHOOTABLE_MASK = LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int ENEMY_LAYER    = LayerMask.NameToLayer("Enemies");
    }
}