using System;
using System.Collections.Generic;

namespace Svelto.ECS.GUI.Resources
{
    public class GUIResources : IDisposable
    {
        public GUIResources()
        {
            _resourceManagers = new Dictionary<uint, IResourceManager>();
            _typeToIds = new Dictionary<Type, uint>();
        }

        public EcsResource ToECS<T>(T resource)
        {
            if (_typeToIds.TryGetValue(typeof(T), out var typeId) == false)
            {
                typeId = (uint)_resourceManagers.Count + 1;
                var resourceManager = new GUIResourceManager<T>(typeId);
                _resourceManagers[typeId] = resourceManager;
                _typeToIds[typeof(T)] = typeId;
            }

            return (_resourceManagers[typeId] as GUIResourceManager<T>).ToECS(resource);
        }

        public T FromECS<T>(EcsResource resource)
        {
            if (resource._type == 0)
            {
                return default;
            }
            return (T)(_resourceManagers[resource._type] as GUIResourceManager<T>).FromECS(resource);
        }

        public void Release(EcsResource resource)
        {
            if (resource._type != 0)
            {
                _resourceManagers[resource._type].Release(resource);
            }
        }

        public void Dispose()
        {
            foreach (var resourceManager in _resourceManagers)
            {
                resourceManager.Value.Dispose();
            }
        }

        readonly Dictionary<Type, uint> _typeToIds;
        readonly Dictionary<uint, IResourceManager> _resourceManagers;
    }
}