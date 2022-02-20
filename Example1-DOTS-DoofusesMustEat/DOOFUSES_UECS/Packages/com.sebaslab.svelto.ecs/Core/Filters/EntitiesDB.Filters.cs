using Svelto.DataStructures;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        public EntityFilterCollection GetPersistentFilter<T>(int filterId)  where T : IEntityComponent
        {
            return _enginesRoot._persistentEntityFilters[EnginesRoot.CombineFilterIDs<T>(filterId)];
        }

        public EntityFilterCollection GetTransientFilter<T>(int filterId)  where T : IEntityComponent
        {
            return _enginesRoot._transientEntityFilters[EnginesRoot.CombineFilterIDs<T>(filterId)];
        }
    }
}