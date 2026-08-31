using System;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    /// <summary>
    /// This abstract has the responsibility to update the object parents when change. Note that this could have
    /// been solved with different strategies. They have all pro and cons:
    /// 1) a simple iteration could have been used, but in order to prevent setting the parent objects even if
    /// they didn't change, an extra if should have been added as well as an extra component field to remember the
    /// old parent
    /// 2) filters could have been used, filtering only the entities that have the parent changed. This would have
    /// added a level of complexity that was useless for this demo
    /// 3) using the publisher/consumer. This assumes that parents do not change often. Even if they change often,
    /// it may not be a big issue; it is all a matter of trade off. The consumer queue grows as required, so a
    /// permanently slower consumer can increase memory usage without bound.
    /// </summary>
    class SyncHierarchyEngine : IStepEngine, IQueryingEntitiesEngine, IDisposable
    {
        public SyncHierarchyEngine(OOPManager oopManager, IEntityStreamConsumerFactory generateConsumerFactory)
        {
            _oopManager = oopManager;
            _consumer =
                generateConsumerFactory.GenerateConsumer<ObjectParentComponent>("SyncHierarchyEngine");
        }

        public void Ready()
        { }

        public void Step()
        {
            while (_consumer.TryDequeue(out var entity, out var id))
            {
                _oopManager.SetParent(entitiesDB.QueryEntity<ObjectIndexComponent>(id).index, entity.parentIndex);
            }
        }

        public   EntitiesDB entitiesDB { get; set; }
        public   string     name => nameof(SyncTransformEngine);

        public   void                   Dispose()   { _consumer.Dispose(); }
 
        readonly OOPManager             _oopManager;
        Consumer<ObjectParentComponent> _consumer;
    }
}
