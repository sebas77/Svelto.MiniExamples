using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    /// This class is a service layer for retrieving ammo spawn data.
    /// </summary>
    public class AmmoSpawnDataService
    {
        public static IEnumerator<JsonAmmoSpawnData> Load()
        {
            var json = Addressables.LoadAssetAsync<TextAsset>("AmmoSpawnData");

            while (json.IsDone == false)
                yield return null;

            JsonAmmoSpawnData ammoSpawnInfo = JsonHelper.getJsonObject<JsonAmmoSpawnData>(json.Result.text);

            yield return ammoSpawnInfo;
        }

    }


    [System.Serializable]
    public struct JsonVector3Serializable
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class JsonAmmoSpawnData
    {
        public JsonVector3Serializable[] spawnLocations;
        public float     secondsBetweenSpawns;
        public int       ammoPerPickup;
        public string    ammoPrefab;
    }
}