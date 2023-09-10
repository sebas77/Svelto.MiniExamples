using Svelto.DataStructures;

namespace Svelto.ECS.GUI.Resources
{
    internal class GUIResourceManager<TManaged> : IResourceManager
    {
        internal GUIResourceManager(uint type)
        {
            _type         = type;
            _items        = new FasterList<TManaged>();
            _itemVersions = new FasterList<uint>();
            _freedIds     = new FasterList<uint>();
        }

        public void Dispose()
        {
            _items.Clear();
            _itemVersions.Clear();
            _freedIds.Clear();
        }

        internal TManaged FromECS(EcsResource resource)
        {
            var id      = resource._id - 1;
            var version = resource._versioning;
            if (id < _items.count && _itemVersions[id] == version)
                return _items[id];

            return default;
        }

        void IResourceManager.Release(EcsResource resource)
        {
            var id      = resource._id - 1;
            var version = resource._versioning;
            if (id < _items.count && _type == resource._type && _itemVersions[id] == version)
            {
                _items[id]        = default;
                _itemVersions[id] = version + 1;
                _freedIds.Add(id + 1);
            }
        }

        internal EcsResource ToECS(TManaged newParams)
        {
            uint id;
            if (_freedIds.count > 0)
            {
                id             = _freedIds[0];
                _items[id - 1] = newParams;
                _freedIds.UnorderedRemoveAt(0);
            }
            else
            {
                _items.Add(newParams);
                _itemVersions.Add(0);
                id = (uint) _items.count;
            }

            var resource = new EcsResource(_type, id, _itemVersions[id - 1]);
            return resource;
        }

        readonly FasterList<uint>     _freedIds;
        readonly FasterList<TManaged> _items;
        readonly FasterList<uint>     _itemVersions;
        readonly uint                 _type;
    }
}