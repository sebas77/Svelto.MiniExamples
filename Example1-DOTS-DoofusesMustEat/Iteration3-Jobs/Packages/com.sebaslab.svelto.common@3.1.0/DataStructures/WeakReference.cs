using System;

/// <span class="code-SummaryComment"><summary></span>
/// Represents a weak reference, which references an object while still allowing
/// that object to be reclaimed by garbage collection.
/// <span class="code-SummaryComment"></summary></span>
/// <span class="code-SummaryComment"><typeparam name="T">The type of the object that is referenced.</typeparam></span>

namespace Svelto.DataStructures
{
    public readonly struct WeakReference<T> where T : class
    {
        public bool     IsValid => _weakReference != null && Target != null && _weakReference.IsAlive == true;

        public T    Target  => (T)_weakReference.Target;
        public bool IsAlive => _weakReference.IsAlive;

        public WeakReference(T target)
        {
            _weakReference = new WeakReference(target);
        }
        
        readonly WeakReference _weakReference;
    }
}
