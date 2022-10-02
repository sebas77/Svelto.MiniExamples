#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Svelto.ECS.Example.Survive.ResourceManager
{
    public class GameObjectFactory
    {
        public IEnumerator<GameObject> Build(string prefabName, bool startActive = true)
        {
            var load = Addressables.LoadAssetAsync<GameObject>(prefabName);

            while (load.IsDone == false) yield return null;

            var go = load.Result;

            var gameObject = GameObject.Instantiate(go);
            gameObject.SetActive(startActive);
            yield return gameObject;
        }
    }
}
#endif