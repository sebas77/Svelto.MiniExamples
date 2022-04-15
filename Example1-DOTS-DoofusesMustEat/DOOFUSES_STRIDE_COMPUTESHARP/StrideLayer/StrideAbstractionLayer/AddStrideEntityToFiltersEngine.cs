namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer
{
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
                //get the Stride entityID that will be instanced multipled times
                var entity = (int)buffer[index].instancingEntity;
                
                //if it doesn't match last one used, let's fetch the filter that is linked to this ID
                if (lastEntity != entity)
                {
                    //I use the stride entityID as filter ID
                    cachedFilter = ref sveltoFilters.GetOrCreatePersistentFilter<StrideComponent>(entity,
                        StrideFilterContext.StrideInstanceContext);
                    
                    lastEntity = entity;
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