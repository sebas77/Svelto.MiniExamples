#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Svelto.ECS.Example.Survive.ResourceManager
{
    /// <summary>
    ///     this is a very rudimentary resource manager, you will need to use similar solutions if you need to
    ///     mix ECS with OOP data. However you must isolate these cases and keep in separate, abstracted layers.
    ///     The user must be clear that these strategies are to solve specific problems and not to be used
    ///     for everything.
    /// </summary>
    public class GameObjectFactory
    {
        public GameObjectFactory() { _prefabs = new Dictionary<string, GameObject>(); }

        public IEnumerator<GameObject> Build(string prefabName, bool startActive = true)
        {
            if (_prefabs.TryGetValue(prefabName, out var go) == false)
            {
                var load = Addressables.LoadAssetAsync<GameObject>(prefabName);

                while (load.IsDone == false) yield return null;

                go = load.Result;
                
                _prefabs.Add(prefabName, go);
            }

            var gameObject = GameObject.Instantiate(go);
            gameObject.SetActive(startActive);
            yield return gameObject;
        }

        readonly Dictionary<string, GameObject> _prefabs;
    }
}
#endif