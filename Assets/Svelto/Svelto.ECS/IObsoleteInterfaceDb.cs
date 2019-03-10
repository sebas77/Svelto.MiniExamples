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
    }
}