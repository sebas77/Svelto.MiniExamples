using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IObsoleteInterfaceDb
    {
        /// <summary>
        /// All the EntityView related methods are left for back compatibility, but
        /// shouldn't be used anymore. Always pick EntityViewStruct or EntityStruct
        /// over EntityView
        /// </summary>
        [Obsolete]
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>(int group) where T : class, IEntityStruct;
        [Obsolete]
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : class, IEntityStruct;
        /// <summary>
        /// All the EntityView related methods are left for back compatibility, but
        /// shouldn't be used anymore. Always pick EntityViewStruct or EntityStruct
        /// over EntityView
        /// </summary>
        [Obsolete]
        bool TryQueryEntityView<T>(EGID egid, out T entityView) where T : class, IEntityStruct;
        [Obsolete]
        bool TryQueryEntityView<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group, out T entityView) where T : class, IEntityStruct;
        /// <summary>
        /// All the EntityView related methods are left for back compatibility, but
        /// shouldn't be used anymore. Always pick EntityViewStruct or EntityStruct
        /// over EntityView
        /// </summary>
        [Obsolete]
        T QueryEntityView<T>(EGID egid) where T : class, IEntityStruct;
        [Obsolete]
        T QueryEntityView<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group) where T : class, IEntityStruct;
        
        /// <summary>
        /// ECS is meant to work on a set of Entities. Working on a single entity is sometime necessary, but us    ing
        /// the following functions inside a loop would be a mistake as performance can be significantly impacted
        /// Execute an action on a specific Entity. Be sure that the action is not capturing variables
        /// otherwise you will allocate memory which will have a great impact on the execution performance
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        [Obsolete]
        void ExecuteOnEntity<T>(EGID egid, EntityAction<T> action) where T : IEntityStruct;
        [Obsolete]
        void ExecuteOnEntity<T>(int id, int groupid, EntityAction<T> action) where T : IEntityStruct;
        [Obsolete]
        void ExecuteOnEntity<T>(int id,  ExclusiveGroup.ExclusiveGroupStruct groupid, EntityAction<T> action) where T : IEntityStruct;
        [Obsolete]
        void ExecuteOnEntity<T, W>(EGID egid, ref W value, EntityAction<T, W> action) where T : IEntityStruct;
        [Obsolete]
        void ExecuteOnEntity<T, W>(int id,  int groupid, ref W value, EntityAction<T, W> action) where T : IEntityStruct;
        [Obsolete]
        void ExecuteOnEntity<T, W>(int id,  ExclusiveGroup.ExclusiveGroupStruct groupid, ref W value, EntityAction<T, W> action) where T : IEntityStruct;

    }
}