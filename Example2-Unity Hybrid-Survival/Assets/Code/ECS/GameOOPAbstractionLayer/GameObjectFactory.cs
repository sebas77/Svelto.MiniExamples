#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    class GameObjectFactory
    {
        internal async Task<GameObject> Build(string prefabName, bool startActive = true)
        {
            var load = await Addressables.LoadAssetAsync<GameObject>(prefabName).Task;

            var go = load.gameObject;

            var gameObject = GameObject.Instantiate(go);
            gameObject.SetActive(startActive);

            return gameObject;
        }
    }
}
#endif