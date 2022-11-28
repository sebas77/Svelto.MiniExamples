using System.Collections.Generic;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Example.Survive.ResourceManager;
using Svelto.ECS.Resources;
using UnityEngine;

namespace Code.ECS.Shared
{
    /// <summary>
    /// Holds the resources used by the game and map them to unmanaged indices usable by Svelto Components
    /// To know more about Resource Managers read: https://www.sebaslab.com/oop-abstraction-layer-in-a-ecs-centric-application/
    /// </summary>
    public class GameObjectResourceManager : ECSResourceManager<GameObject>
    {
        public GameObjectResourceManager() : base()
        {
            _factory = new GameObjectFactory();
        }

        public IEnumerable<ValueIndex?> Build(string prefabName)
        {
            var gameObject = _factory.Build(prefabName);

            while (gameObject.MoveNext()) 
                yield return null;

            yield return Add(gameObject.Current);
        }

        readonly GameObjectFactory _factory;
    }
}