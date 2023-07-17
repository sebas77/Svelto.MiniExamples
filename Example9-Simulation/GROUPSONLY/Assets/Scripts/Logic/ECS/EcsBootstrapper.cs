using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace ECS
{
    /**
     * Bootstraps worlds with their corresponding systems and queues dependency injection on the SystemBase systems
     */
    public class EcsBootstrapper : MonoBehaviour
    {
        private World customWorld;

        static bool IsCustomSystem(Type _systemType)
        {
            var nonce = _systemType.Namespace.Split(".");
            return nonce[0] == "Logic" && nonce[1] == "ECS";
        }

        static bool IsNormalUnitySystem(Type _systemType)
        {
            return (_systemType.Namespace.Split(".")[0] == "Unity" ||
                    _systemType.Name.Contains("Companion")) &&
                   _systemType.Namespace.Split(".")[1] != "Physics";
        }

        private static bool IsDisabledSystem(Type _system)
        {
            return SystemHasAttribute(_system, typeof(DisableAutoCreationAttribute));
        }

        private static bool SystemHasAttribute(Type _system, Type _attribute)
        {
            return TypeManager.GetSystemAttributes(_system, _attribute)?.Length > 0;
        }

        private void Start()
        {
            // this seems to be necessary in development builds and on some
            // systems, otherwise people get this error:
            // "The TypeManager must be initialized before the TypeManager can be used"
            TypeManager.Initialize();

            customWorld = CreateWorld();

            // start updating worlds
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(customWorld);
        }

        private void OnDestroy()
        {
            customWorld.Dispose();
        }

        private static World CreateGenericWorld(List<Type> _systems)
        {
            var world = new World("Custom World");
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, _systems);
            return world;
        }

        private static World CreateWorld()
        {
            var customSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default)
                .Where(_system =>
                    (IsNormalUnitySystem(_system) || IsCustomSystem(_system)) && !IsDisabledSystem(_system)
                ).ToList();

            return CreateGenericWorld(customSystems);
        }
    }
}