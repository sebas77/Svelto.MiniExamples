using Stride.Core.Mathematics;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.Rendering;
using Svelto.DataStructures;

namespace Svelto.ECS.MiniExamples.Doofuses.Stride.StrideLayer
{
    /// <summary>
    /// Note: This class looks weird and you may wonder where stride objects are instantiated. I am using an
    /// advanced technique to exploit GPU instancing. This stride API is not intuitive and you need to know
    /// how it works to be able to read the code. The technique needs just one stride object to be instantiated
    /// while the array of transforms is set to a specific component of this object so that instancing can be enabled
    /// </summary>
    public class ECSStrideEntityManager
    {
        public ECSStrideEntityManager(IContentManager contentManager, SceneSystem sceneSystem)
        {
            _contentManager = contentManager;
            _sceneSystem    = sceneSystem;
        }

        public uint InstantiateInstancingEntity(uint prefabID)
        {
            var entity = _prefabEntities[prefabID].Instantiate()[0];
            entity.Transform.UseTRS = false;
            _sceneSystem.SceneInstance.RootScene.Entities.Add(entity);

            _entities.Add(entity);

            return _entityCount++;
        }
        
        public Matrix[] GetInstancingTransformations(uint entityID)
        {
            return (_entities[entityID].Get<InstancingComponent>().Type as InstancingUserArray).WorldMatrices;
        }

        public void SetInstancingTransformations(uint entityID, Matrix[] matrices, int actualCount)
        {
            (_entities[entityID].Get<InstancingComponent>().Type as InstancingUserArray).UpdateWorldMatrices(matrices,
                actualCount);
        }

        //load a prefab resource and register it as a prefab. Of course this method is very naive and can be made
        //async and suitable to load several prefabs at once.
        public uint LoadAndRegisterPrefab(string prefabName, out Prefab prefab)
        {
            prefab = _contentManager.Load<Prefab>(prefabName);
            _prefabEntities.Add(prefab);
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
        readonly SceneSystem        _sceneSystem;
    }
}