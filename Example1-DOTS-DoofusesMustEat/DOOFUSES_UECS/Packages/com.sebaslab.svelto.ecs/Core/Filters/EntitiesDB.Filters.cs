using Svelto.DataStructures;
using Svelto.ECS.Native;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        public SveltoFilters GetFilters()
        {
            return new SveltoFilters(_enginesRoot);
        }

        /// <summary>
        /// I want to write this in such a way it can be passed and used inside a Unity Job
        /// </summary>
        public readonly ref struct SveltoFilters
        {
            readonly EnginesRoot _enginesRoot;

            public SveltoFilters(EnginesRoot enginesRoot)
            {
                _enginesRoot = enginesRoot;
            }

            /// <summary>
            /// Create a persistent filter. Persistent filters are not deleted after each submission,
            /// however they have a maintenance cost that must be taken into account and will affect
            /// entities submission performance.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(int filterID) 
                where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs                   = EnginesRoot.CombineFilterIDs<T>(filterID);
                var  enginesRootPersistentEntityFilters = _enginesRoot._persistentEntityFilters;
                
                if (enginesRootPersistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref enginesRootPersistentEntityFilters.GetDirectValueByRef(index);
                
                var typeRef          = TypeRefWrapper<T>.wrapper;
                var filterCollection = EntityFilterCollection.Create();

                enginesRootPersistentEntityFilters.Add(combineFilterIDs,
                    filterCollection);
                _enginesRoot._persistentFilters.Add(filterCollection);

                _enginesRoot._indicesOfPersistentFiltersUsedByThisComponent
                   .GetOrAdd(typeRef, () => new FasterList<int>())
                   .Add(_enginesRoot._persistentFilters.count - 1);

                ref var orCreatePersistentFilter = ref _enginesRoot._persistentFilters[
                    (uint)(_enginesRoot._persistentFilters.count - 1)];
                
                return ref orCreatePersistentFilter;
            }
            
            /// <summary>
            /// Creates a transient filter. Transient filters are deleted after each submission
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ref EntityFilterCollection GetOrCreateTransientFilter<T>(int filterID)
                where T : unmanaged, IEntityComponent
            {
                var combineFilterIDs                  = EnginesRoot.CombineFilterIDs<T>(filterID);
                var enginesRootTransientEntityFilters = _enginesRoot._transientEntityFilters;
                
                if (enginesRootTransientEntityFilters.TryFindIndex(combineFilterIDs, out var index))
                    return ref enginesRootTransientEntityFilters.GetDirectValueByRef(index);
                
                var filterCollection = EntityFilterCollection.Create();

                enginesRootTransientEntityFilters.Add(combineFilterIDs,
                    filterCollection);
                _enginesRoot._transientFilters.Add(filterCollection);
                
                return ref enginesRootTransientEntityFilters.GetDirectValueByRef(
                    (uint)(enginesRootTransientEntityFilters.count - 1));
            }
        }
    }
}