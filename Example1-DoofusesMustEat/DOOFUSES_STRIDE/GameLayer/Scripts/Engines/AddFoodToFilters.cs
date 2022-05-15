// namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp
// {
//     public class AddFoodToFilters : IReactOnAddAndRemoveEx<MealInfoComponent>, IQueryingEntitiesEngine
//     {
//         public void Add
//         ((uint start, uint end) rangeOfEntities, in EntityCollection<MealInfoComponent> collection
//        , ExclusiveGroupStruct groupID)
//         {
//             var filters = entitiesDB.GetFilters()
//                                     .GetOrCreatePersistentFilter<MealInfoComponent>(
//                                          Filters.Meals, Filters.MealContextID);
//
//             var (_, ids, _) = collection;
//
//             for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; ++i)
//             {
//                 filters.Add(ids[i], groupID, i);
//             }
//         }
//
//         public void Remove
//         ((uint start, uint end) rangeOfEntities, in EntityCollection<MealInfoComponent> collection
//        , ExclusiveGroupStruct groupID)
//         {
//             var filters = entitiesDB.GetFilters()
//                                     .GetOrCreatePersistentFilter<MealInfoComponent>(
//                                          Filters.Meals, Filters.MealContextID);
//
//             var (_, ids, _) = collection;
//
//             for (uint i = rangeOfEntities.start; i < rangeOfEntities.end; ++i)
//             {
//                 filters.Remove(ids[i], groupID);
//             }
//         }
//
//         public void Ready() { }
//
//         public EntitiesDB entitiesDB { get; set; }
//     }
// }