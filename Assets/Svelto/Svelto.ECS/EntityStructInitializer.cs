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
            ID      = id;
        }
        
        public EGID ID { get; }

        public void Init<T>(ref T initializer) where T: struct, IEntityStruct
        {
            if (_group.TryGetValue(typeof(T), out var typeSafeDictionary) == true)
            {
                var dictionary = typeSafeDictionary as TypeSafeDictionary<T>;
                
                initializer.ID = ID;

                if (dictionary.TryFindElementIndex(ID.entityID, out var findElementIndex))
                    dictionary.GetValuesArray(out _)[findElementIndex] = initializer;
            }
        }
        
        public void Init<T>(T initializer) where T: struct, IEntityStruct
        {
            Init(ref initializer);
        }

        readonly Dictionary<Type, ITypeSafeDictionary> _group;
    }
}