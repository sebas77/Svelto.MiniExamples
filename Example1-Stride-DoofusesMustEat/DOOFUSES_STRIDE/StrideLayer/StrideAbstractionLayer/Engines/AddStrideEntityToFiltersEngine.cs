namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample.StrideLayer
{
    /// <summary>
    /// Handle the filters of instanced objects. For each prefab, there is a filter set to encompass the instances
    /// of that specific prefab. The goal is in fact to be able to identify instances of a specific prefab through
    /// its filter.
    /// Swap and Remove of persistent filters are automatically handled by the framework
    /// </summary>
    class AddStrideEntityToFiltersEngine : IReactOnAddEx<StrideComponent>, IQueryingEntitiesEngine
    {
        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<StrideComponent> collection,
                        ExclusiveGroupStruct groupID)
        {
            var (buffer, entityIDs, _) = collection;
 
            //Fetch the new Svelto.ECS filters
            var sveltoFilters = entitiesDB.GetFilters();
 
            int     lastEntity   = -1;
            ref var cachedFilter = ref _default;
 
            //for each entity added in this submission phase
            for (uint index = rangeOfEntities.start; index < rangeOfEntities.end; index++)
            {
                //get the Stride entityID that will be instanced multiple times
                var prefabId = (int)buffer[index].prefabID;
                
                //if it doesn't match last one used, let's fetch the filter that is linked to this ID
                if (lastEntity != prefabId)
                {
                    if (buffer[index].updateOnce == false)
                        //I use the stride entityID as filter ID
                        cachedFilter = sveltoFilters.GetOrCreatePersistentFilter<MatrixComponent>(prefabId,
                            StrideFilterContext.StridePrefabFilterContextUpdate);
                    else
                            //I use the stride entityID as filter ID
                        cachedFilter = sveltoFilters.GetOrCreatePersistentFilter<MatrixComponent>(prefabId,
                            StrideFilterContext.StridePrefabFilterContextUpdate);
                    
                    lastEntity = prefabId;
                }
 
                //add the current entity instance to the filter linked to the prefab entity that will be instanced
                cachedFilter.Add(entityIDs[index], groupID, index);
            }
        }
 
        public void Ready() { }
 
        public EntitiesDB entitiesDB { get; set; }
 
        EntityFilterCollection _default;
    }
}