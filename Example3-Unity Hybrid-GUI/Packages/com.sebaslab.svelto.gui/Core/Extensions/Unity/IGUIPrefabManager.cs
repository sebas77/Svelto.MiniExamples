using UnityEngine;

namespace Svelto.ECS.GUI.Extensions.Unity
{
    public interface IGUIPrefabManager
    {
        uint GetPrefabId(string name);

        GameObject InstantiatePrefab(uint prefabId, Transform parent, out uint instanceId);

        void Recycle(uint prefabId, uint instanceId);
    }
}