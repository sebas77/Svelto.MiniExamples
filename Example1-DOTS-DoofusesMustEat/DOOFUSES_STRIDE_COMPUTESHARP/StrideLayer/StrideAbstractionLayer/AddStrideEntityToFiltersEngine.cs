namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer
{
    class AddStrideEntityToFiltersEngine : IReactOnAddEx<StrideComponent>, IQueryingEntitiesEngine
    {
        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<StrideComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var (buffer, entityIDs, _) = collection;

            var sveltoFilters = entitiesDB.GetFilters();

            int     _lastEntity              = -1;
            ref var orCreatePersistentFilter = ref _default;

            for (uint index = rangeOfEntities.start; index < rangeOfEntities.end; index++)
            {
                var entity = (int)buffer[index].instancingEntity;

                if (_lastEntity != entity)
                {
                    orCreatePersistentFilter = ref sveltoFilters.GetOrCreatePersistentFilter<StrideComponent>(entity,
                        StrideFilterContext.StrideInstanceContext);
                    _lastEntity = entity;
                }

                orCreatePersistentFilter.Add(entityIDs[index], groupID, index);
            }
        }

        public void Ready()
        {
        }

        public EntitiesDB entitiesDB { get; set; }

        EntityFilterCollection _default;
    }
}