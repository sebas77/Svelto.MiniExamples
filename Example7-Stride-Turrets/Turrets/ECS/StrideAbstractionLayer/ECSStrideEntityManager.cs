using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Svelto.DataStructures;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public class ECSStrideEntityManager
    {
        public ECSStrideEntityManager(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        //Instantiate and register a new entity from a previously registered prefab
        public uint InstantiateEntity(uint prefabID, bool useTRS)
        {
            var entity = _prefabEntities[prefabID].Instantiate()[0];
            _entities.Add(entity);
            entity.Transform.UseTRS = useTRS;
            return _entityCount++;
        }
        
        //register an already existing and Stride pre-created entity
        public uint RegisterStrideEntity(Entity entity)
        {
            _entities.Add(entity);
            return _entityCount++;
        }

        //convert a svelto compatible ID to an Entity
        public Entity GetStrideEntity(uint entityID)
        {
            return _entities[entityID];
        }

        //load a prefab resource and register it as a prefab. Of course this method is very naive and can be made
        //async and suitable to load several prefabs at once.
        public uint LoadAndRegisterPrefab(string prefabName, out Matrix prefabTransform)
        {
            var prefab = _contentManager.Load<Prefab>(prefabName);
            _prefabEntities.Add(prefab);
            prefabTransform = prefab.Entities[0].Transform.LocalMatrix;
            return _prefabsCount++;
        }
        
        public void Dispose()
        {
        }

        uint                        _entityCount;
        uint                        _prefabsCount;
        readonly FasterList<Entity> _entities       = new();
        readonly FasterList<Prefab> _prefabEntities = new();
        readonly IContentManager    _contentManager;
    }
}