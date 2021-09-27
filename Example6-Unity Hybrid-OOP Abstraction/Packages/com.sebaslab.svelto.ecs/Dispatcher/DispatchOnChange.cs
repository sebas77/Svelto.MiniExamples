using System;

namespace Svelto.ECS
{
    public class DispatchOnChange<T> : DispatchOnSet<T> where T : IEquatable<T>
    {
        public DispatchOnChange(EGID senderID, T initialValue = default) : base(senderID, initialValue) { }

        public DispatchOnChange
            (EGID senderID, Action<EGID, T> callback, T initialValue = default, bool notifyImmediately = false) :
            base(senderID, callback, initialValue, notifyImmediately) { }

        public new T value
        {
            set
            {
                if (value.Equals(_value) == false)
                    base.value = value;
            }
            get => _value;
        }
    }
}