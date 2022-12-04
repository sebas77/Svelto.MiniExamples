using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures.Native;

namespace Svelto.DataStructures
{
    public struct SharedDictionaryStruct
    {
        public IntPtr test;

        SveltoDictionary<long, Sentinel, NativeStrategy<SveltoDictionaryNode<long>>,
            NativeStrategy<Sentinel>, NativeStrategy<int>> cast
        {
            get
            {
                unsafe
                {
                    return Unsafe
                       .AsRef<SveltoDictionary<long, Sentinel, NativeStrategy<SveltoDictionaryNode<long>>,
                            NativeStrategy<Sentinel>, NativeStrategy<int>>>((void*)test);
                }
            }
        }


        public void Add(long ptr, Sentinel sentinel)
        {
            cast.Add(ptr, sentinel);
        }

        public ref Sentinel GetValueByRef(long ptr)
        {
            return ref cast.GetValueByRef(ptr);
        }

        public bool Exists(IntPtr ptr)
        {
            return cast.TryFindIndex((long)ptr, out _);
        }

        public static IntPtr Create()
        {
            unsafe
            {
                //allocate the pointer to the dictionary
                IntPtr dic =  MemoryUtilities
                   .NativeAlloc<SveltoDictionary<long, Sentinel, NativeStrategy<SveltoDictionaryNode<long>>,
                        NativeStrategy<Sentinel>, NativeStrategy<int>>>(1, Allocator.Persistent);
            
                //allocate the dictionary itself
                Unsafe.AsRef<SveltoDictionary<long, Sentinel, NativeStrategy<SveltoDictionaryNode<long>>,
                        NativeStrategy<Sentinel>, NativeStrategy<int>>>((void*)dic) =
                    new SveltoDictionary<long, Sentinel, NativeStrategy<SveltoDictionaryNode<long>>,
                        NativeStrategy<Sentinel>, NativeStrategy<int>>(1, Allocator.Persistent);

                return dic;
            }
        }
    }
}