using System;

namespace Svelto.ECS
{
    /// <summary>
    /// Reasons why unfortunately this cannot be a struct:
    /// the user must remember to create interface with ref getters
    /// ref getters cannot have set, while we sometimes use set to initialise values
    /// the struct will be valid even if it has not ever been initialised
    ///
    /// 1 and 3 are possibly solvable, but 2 is a problem
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DispatchOnSet<T>
    {
        public DispatchOnSet
        (EntityReference senderID, Action<EntityReference, T> callback, T initialValue = default(T)
       , bool notifyImmediately = false)
        {
            _subscriber = callback;

            if (notifyImmediately)
                _subscriber(_senderID, initialValue);

            _senderID = senderID;
            _value    = initialValue;
        }

        public T value
        {
            set
            {
                if (_paused == false)
                    _subscriber(_senderID, value);

                //all the subscribers relies on the actual value not being changed yet, as the second parameter
                //is the new value
                _value = value;
            }
            get => _value;
        }

        public void SetValueWithoutNotify(in T value)
        {
            _value = value;
        }

        public void StopNotify()
        {
            _subscriber = null;
            _paused     = true;
        }

        public void PauseNotify()
        {
            _paused = true;
        }

        public void ResumeNotify()
        {
            _paused = false;
        }

        protected T               _value;
        readonly  EntityReference _senderID;

        Action<EntityReference, T> _subscriber;
        bool                       _paused;
    }
}