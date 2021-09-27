using System;

namespace Svelto.ECS.Hybrid
{
    public static class HybridDispatchOnSetClone
    {
        public static DispatchOnChange<uint> InterceptDispatchOnChange<T>
        (this T implementor, DispatchOnChange<uint> dispatchOnChange
       , Action<EGID, uint> onDropListIndexChangedExternally) where T:IImplementor, new()
        {
            throw new Exception();
        }
    }
}