using System;

namespace Svelto.ECS
{
    public class ReactiveValue<T> : DispatchOnSet<T> where T : IEquatable<T>
    {
        public ReactiveValue
        (EntityReference senderID, Action<EntityReference, T> callback, T initialValue = default
       , bool notifyImmediately = false) : base(senderID, callback, initialValue, notifyImmediately) { }

        public new T value
        {
            set
            {
                if (value.Equals(_value) == false)
                    base.value = value;
            }
            get => _value;
        }

        public void ForceValue(in T value)
        {
            base.value = value;
        }
    }
}