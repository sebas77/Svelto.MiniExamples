using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public static class ExclusiveBuildGroupExtensions
    {
        internal static FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>
            _removeTransitions =
                new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>();

        internal static FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>
            _addTransitions =
                new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>();

        internal static FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>
            _swapTransitions =
                new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>();

        public static void SetTagAddition<T>
            (this ExclusiveBuildGroup group, ExclusiveBuildGroup target, bool setReverse = true) where T : GroupTag<T>
        {
            if (_addTransitions.TryGetValue(@group, out var transitions) == false)
            {
                transitions             = new FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>();
                _addTransitions[@group] = transitions;
            }

            var type = new RefWrapper<Type>(typeof(T));
            transitions[type] = target;

            if (setReverse)
            {
                SetTagRemoval<T>(target, group, false);
            }
        }

        public static void SetTagRemoval<T>
            (this ExclusiveBuildGroup group, ExclusiveBuildGroup target, bool setReverse = true) where T : GroupTag<T>
        {
            if (_removeTransitions.TryGetValue(@group, out var transitions) == false)
            {
                transitions                = new FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>();
                _removeTransitions[@group] = transitions;
            }

            var type = new RefWrapper<Type>(typeof(T));
            transitions[type] = target;

            if (setReverse)
            {
                SetTagAddition<T>(target, group, false);
            }
        }

        public static void SetTagSwap<TRemove, TAdd>
            (this ExclusiveBuildGroup group, ExclusiveBuildGroup target, bool setReverse = true)
            where TRemove : GroupTag<TRemove> where TAdd : GroupTag<TAdd>
        {
            if (_swapTransitions.TryGetValue(@group, out var transitions) == false)
            {
                transitions             = new FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>();
                _swapTransitions[group] = transitions;
            }

            var type = new RefWrapper<Type>(typeof(TAdd));
            transitions[type] = target;

            // To avoid needing to check if the group already has the tag when swapping (prevent ecs exceptions).
            // The current groups adds the removed tag pointing to itself.
            type              = new RefWrapper<Type>(typeof(TRemove));
            transitions[type] = group;

            if (setReverse)
            {
                SetTagSwap<TAdd, TRemove>(target, group, false);
            }
        }
    }
}