#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Svelto.ECS.Example.Survive.ResourceManager
{
    /// <summary>
    ///     this is a very rudimentary resource manager, you will need to use similar solutions if you need to
    ///     mix ECS with OOP data.
    /// </summary>
    public class GameObjectFactory
    {
        public GameObjectFactory() { _prefabs = new Dictionary<string, GameObject>(); }

        public IEnumerator<GameObject> Build(string prefabName)
        {
            if (_prefabs.TryGetValue(prefabName, out var go) == false)
            {
                var load = Addressables.LoadAsset<GameObject>(prefabName);

                while (load.IsDone == false) yield return null;

                go = load.Result;
            }

            yield return GameObject.Instantiate(go);
        }

        readonly Dictionary<string, GameObject> _prefabs;
    }
}
#endif