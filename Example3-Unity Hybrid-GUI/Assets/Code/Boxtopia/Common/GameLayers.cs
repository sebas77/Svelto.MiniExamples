using UnityEngine;

namespace Common
{
    static class GameLayers
    {
        public static readonly int UI3d = LayerMask.NameToLayer("3dUI");
        public readonly static int DEFAULT = LayerMask.NameToLayer("Default");
        public readonly static int TRANSPARENTFX = LayerMask.NameToLayer("TransparentFX");
        public readonly static int IGNORE_RAYCAST = LayerMask.NameToLayer("Ignore Raycast");
        public readonly static int WATER = LayerMask.NameToLayer("Water");
        public readonly static int UI = LayerMask.NameToLayer("UI");
        public readonly static int TERRAIN = LayerMask.NameToLayer("Terrain");
        public readonly static int PROPS = LayerMask.NameToLayer("Props");
        public readonly static int LOCAL_PLAYER = LayerMask.NameToLayer("LocalPlayer");

        public readonly static int CAMERA_COLLISION_MASK = (1 << TERRAIN) | (1 << PROPS);
        public readonly static int LOCAL_PLAYER_COLLISION_MASK = (1 << TERRAIN) | (1 << PROPS);

        public static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;

            Transform t = obj.transform;
            for (int i = 0; i < t.childCount; ++i)
            {
                Transform child = t.GetChild(i);
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}