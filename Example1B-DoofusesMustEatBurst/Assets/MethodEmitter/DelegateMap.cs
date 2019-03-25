using System;
using System.Collections.Generic;

namespace MethodEmitter
{
    public static class DelegateMap
    {
        public static IReadOnlyDictionary<int, Type> ActionsByParameterNumber = new Dictionary<int, Type>(17) {
            { 0, typeof(Action)},
            { 1, typeof(Action<>)},
            { 2, typeof(Action<,>)},
            { 3, typeof(Action<,,>)},
            { 4, typeof(Action<,,,>)},
            { 5, typeof(Action<,,,,>)},
            { 6, typeof(Action<,,,,,>)},
            { 7, typeof(Action<,,,,,,>)},
            { 8, typeof(Action<,,,,,,,>)},
            { 9, typeof(Action<,,,,,,,,>)},
            { 10, typeof(Action<,,,,,,,,,>)},
            { 11, typeof(Action<,,,,,,,,,,>)},
            { 12, typeof(Action<,,,,,,,,,,,>)},
            { 13, typeof(Action<,,,,,,,,,,,,>)},
            { 14, typeof(Action<,,,,,,,,,,,,,>)},
            { 15, typeof(Action<,,,,,,,,,,,,,,>)},
            { 16, typeof(Action<,,,,,,,,,,,,,,,>)}
        };

        public static IReadOnlyDictionary<int, Type> FuncsByParameterNumber = new Dictionary<int, Type>(17) {
            { 1, typeof(Func<>)},
            { 2, typeof(Func<,>)},
            { 3, typeof(Func<,,>)},
            { 4, typeof(Func<,,,>)},
            { 5, typeof(Func<,,,,>)},
            { 6, typeof(Func<,,,,,>)},
            { 7, typeof(Func<,,,,,,>)},
            { 8, typeof(Func<,,,,,,,>)},
            { 9, typeof(Func<,,,,,,,,>)},
            { 10, typeof(Func<,,,,,,,,,>)},
            { 11, typeof(Func<,,,,,,,,,,>)},
            { 12, typeof(Func<,,,,,,,,,,,>)},
            { 13, typeof(Func<,,,,,,,,,,,,>)},
            { 14, typeof(Func<,,,,,,,,,,,,,>)},
            { 15, typeof(Func<,,,,,,,,,,,,,,>)},
            { 16, typeof(Func<,,,,,,,,,,,,,,,>)},
            { 17, typeof(Func<,,,,,,,,,,,,,,,,>)}
        };
    }
}
