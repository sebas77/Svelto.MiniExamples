using System;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    class SyncHierarchyEngine : IStepEngine, IQueryingEntitiesEngine, IDisposable
    {
        public SyncHierarchyEngine(OOPManager oopManager, IEntityStreamConsumerFactory generateConsumerFactory, 
                                   uint maxQuantity)
        {
            _oopManager = oopManager;
            _consumer =
                generateConsumerFactory.GenerateConsumer<ObjectParentComponent>("SyncHierarchyEngine", maxQuantity);
        }

        public void Ready()
        { }

        public void Step()
        {
            while (_consumer.TryDequeue(out var entity, out var id))
            {
                _oopManager.SetParent(entitiesDB.QueryEntity<ObjectIndexComponent>(id).index, entity.parent);
            }
        }

        public   EntitiesDB entitiesDB { get; set; }
        public   string     name => nameof(SyncTransformEngine);

        public   void                   Dispose()   { _consumer.Dispose(); }
 
        readonly OOPManager             _oopManager;
        Consumer<ObjectParentComponent> _consumer;
    }
}