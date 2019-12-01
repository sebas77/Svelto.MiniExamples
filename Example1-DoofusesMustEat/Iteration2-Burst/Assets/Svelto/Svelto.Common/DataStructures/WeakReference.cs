using System;

/// <span class="code-SummaryComment"><summary></span>
/// Represents a weak reference, which references an object while still allowing
/// that object to be reclaimed by garbage collection.
/// <span class="code-SummaryComment"></summary></span>
/// <span class="code-SummaryComment"><typeparam name="T">The type of the object that is referenced.</typeparam></span>

namespace Svelto.DataStructures
{
    public class WeakReference<T>: WeakReference where T : class
    {
        public bool IsValid => Target != null && IsAlive == true;

        public new T Target => (T)base.Target;

        public WeakReference(T target) : base(target)
        {}
    }
}
