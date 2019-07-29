using System;
using System.Collections.Generic;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EntityStructInitializer
    {
        public EntityStructInitializer(EGID id, Dictionary<Type, ITypeSafeDictionary> @group)
        {
            _group = @group;
            _ID      = id;
        }

        public void Init<T>(T initializer) where T: struct, IEntityStruct
        {
            if (_group.TryGetValue(EntityBuilder<T>.ENTITY_VIEW_TYPE, out var typeSafeDictionary) == true)
            {
                var dictionary = typeSafeDictionary as TypeSafeDictionary<T>;

                if (EntityBuilder<T>.HAS_EGID)
                    (initializer as INeedEGID).ID = _ID;

                if (dictionary.TryFindIndex(_ID.entityID, out var findElementIndex))
                    dictionary.GetDirectValue(findElementIndex) = initializer;
            }
        }
        
        readonly EGID                                  _ID;
        readonly Dictionary<Type, ITypeSafeDictionary> _group;
    }
}