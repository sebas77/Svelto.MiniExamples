using System.Collections.Generic;
using Svelto.ECS.Example.Survive.ResourceManager;
using UnityEngine;

namespace Code.ECS.Shared
{
    public class GameObjectResourceManager : ECSResourceManager<GameObject>
    {
        public GameObjectResourceManager() : base()
        {
            _factory = new GameObjectFactory();
        }

        public IEnumerator<uint?> Build(string prefabName)
        {
            var gameObject = _factory.Build(prefabName);

            while (gameObject.MoveNext()) 
                yield return null;

            yield return Add(gameObject.Current);
        }

        readonly GameObjectFactory _factory;
    }
}