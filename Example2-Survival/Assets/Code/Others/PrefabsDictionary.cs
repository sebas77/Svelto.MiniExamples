using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Svelto.ECS.Example.Survive
{
    public class PrefabsDictionary
    {
        readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

        public PrefabsDictionary()
        {
            var json = File.ReadAllText("prefabs.json");

            var gameobjects = JsonHelper.getJsonArray<GameObject>(json);

            for (var i = 0; i < gameobjects.Length; i++) prefabs[gameobjects[i].name] = gameobjects[i];
        }

        public GameObject Istantiate(string player) { return Object.Instantiate(prefabs[player]); }
    }
}