using System;
using System.Collections.Generic;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;
using T =
    Svelto.DataStructures.Experimental.FasterDictionary<uint, System.Collections.Generic.Dictionary<System.Type,
        Svelto.ECS.Internal.ITypeSafeDictionary>>;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class DoubleBufferedEntitiesToAdd
        {
            readonly T _entityViewsToAddBufferA = new T();
            readonly T _entityViewsToAddBufferB = new T();

            internal DoubleBufferedEntitiesToAdd()
            {
                other = _entityViewsToAddBufferA;
                current = _entityViewsToAddBufferB;
            }

            internal T other;
            internal T current;
            internal uint entitiesBuiltThisSubmission;

            internal void Swap()
            {
                var toSwap = other;
                other = current;
                current = toSwap;
            }

            /// <summary>
            /// I will need to pool the groups here instead to recreate every time
            /// </summary>
            public void ClearOther()
            {
                foreach (var item in other)
                {
                    foreach (var subitem in item.Value)
                    {
                        subitem.Value.Clear();
                    }

                    item.Value.Clear();
                }

                other.Clear();
                
                entitiesBuiltThisSubmission = 0;
            }
        }
    }
}