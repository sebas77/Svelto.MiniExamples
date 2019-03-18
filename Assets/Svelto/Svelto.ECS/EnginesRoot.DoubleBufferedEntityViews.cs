using System;
using Svelto.DataStructures.Experimental;
using T =
    Svelto.DataStructures.Experimental.FasterDictionary<uint, System.Collections.Generic.Dictionary<System.Type,
        Svelto.ECS.Internal.ITypeSafeDictionary>>;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        internal class DoubleBufferedEntitiesToAdd
        {
            readonly T _entityViewsToAddBufferA = new T();
            readonly T _entityViewsToAddBufferB = new T();
            readonly internal FasterDictionary<uint, uint> entitiesCreatedPerGroup = new FasterDictionary<uint, uint>();

            internal DoubleBufferedEntitiesToAdd()
            {
                other = _entityViewsToAddBufferA;
                current = _entityViewsToAddBufferB;
            }

            internal T other;
            internal T current;
            
            internal void Swap()
            {
                var toSwap = other;
                other = current;
                current = toSwap;
            }

            public void ClearOther()
            {
                foreach (var item in other)
                {
                    foreach (var subitem in item.Value)
                    {
                        subitem.Value.Clear();
                    }
                }

                entitiesCreatedPerGroup.Clear();
            }
        }
    }
}