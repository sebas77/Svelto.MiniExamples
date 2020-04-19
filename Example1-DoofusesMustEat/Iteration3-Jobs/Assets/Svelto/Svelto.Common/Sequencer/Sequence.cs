using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;

namespace Svelto.Common
{
    public interface ISequenceOrder
    {
        string[] enginesOrder { get; }
    }
    
    static class SequenceCache<SequenceOrder> where SequenceOrder : struct, ISequenceOrder
    {
        static SequenceCache()
        {
            cachedEnum = new Dictionary<string, int>();

            string[] values  = new SequenceOrder().enginesOrder;
            int counter = 0;
            
            DBC.Common.Check.Require(values != null, $"The sequence array {typeof(SequenceOrder).Name} hasn't been properly setup");            

            foreach (var name in values)
            {
                try
                {
                    cachedEnum.Add(name, counter++);
                }
                catch
                {
                    throw new Exception($"Order Sequence {typeof(SequenceOrder).Name} has duplicated entry {name}");
                }
            }
        }

        internal static Dictionary<string, int> cachedEnum;
    }
    
    public class Sequence<T, En> where En : struct, ISequenceOrder where T:class
    {
        public Sequence(FasterReadOnlyList<T> itemsToSort)
        {
            _ordered = FasterList<T>.PreInit((uint) SequenceCache<En>.cachedEnum.Count);
            int counted = 0;
            var cachedEnum = SequenceCache<En>.cachedEnum;
            foreach (T item in itemsToSort)
            {
                var type = item.GetType();
                
                DBC.Common.Check.Require(type.IsDefined(typeof(SequencedAttribute)) == true
                        , $"Sequenced item not tagged as Sequenced {type.Name}");
                string typeName = type.GetCustomAttribute<SequencedAttribute>().name;
                DBC.Common.Check.Require(cachedEnum.ContainsKey(typeName) == true
                    , $"Sequenced item not contained in the sequence {typeName}");
                    var index = cachedEnum[typeName];
                    counted++;
                DBC.Common.Check.Require(_ordered[index] == null, $"Items to sequence contains duplicate, {typeName} (wrong sequenced attribute name?)");
                _ordered[index] = item;
            }
            
#if DEBUG && !PROFILE_SVELTO
            if (counted != cachedEnum.Count)
            {
                HashSet<string> debug = new HashSet<string>();

                foreach (T debugItem in itemsToSort)
                {
                    debug.Add(debugItem.GetType().Name);
                }

                foreach (var debugSequence in cachedEnum.Keys)
                {
                    if (debug.Contains(debugSequence) == false)
                        throw new Exception(
                            $"The sequence {typeof(En).Name} wasn't fully satisfied, missing sequenced item {debugSequence}");
                }
            }
#endif
        }

        readonly FasterList<T> _ordered;
        public FasterReadOnlyList<T> items => new FasterReadOnlyList<T>(_ordered);
    }

    public class SequencedAttribute: Attribute
    {
        public SequencedAttribute(string name)
        {
            this.name = name;
        }
        
        internal string name { get; }
    }
}