using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public static class GAME_LAYERS
    {
        public static readonly int SHOOTABLE_MASK     = LayerMask.GetMask("Shootable");
        public static readonly int ENEMY_MASK         = LayerMask.GetMask("Enemies");
        
        public static readonly int NOT_SHOOTABLE_LAYER = LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int ENEMY_LAYER        = LayerMask.NameToLayer("Enemies");
        public static readonly int PLAYER_LAYER  = LayerMask.NameToLayer("Player");
    }
}