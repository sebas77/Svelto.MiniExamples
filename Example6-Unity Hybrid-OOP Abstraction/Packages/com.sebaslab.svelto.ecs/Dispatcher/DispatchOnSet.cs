using System;

namespace Svelto.ECS
{
    public class DispatchOnSet<T>
    {
        public DispatchOnSet
            (EGID senderID, Action<EGID, T> callback, T initialValue = default(T), bool notifyImmediately = false) :
            this(senderID, initialValue)
        {
            _subscriber = callback;
            
            if (notifyImmediately)
                callback(_senderID, initialValue);
        }

        public DispatchOnSet(EGID senderID, T initialValue = default(T))
        {
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
        }

        public void SetValueWithoutNotify(in T value)
        {
            _value = value;
        }

        /// <summary>
        /// This method must be removed, by design DispatchOnChange/Set can be only used by the engine that
        /// creates it
        /// </summary>
        /// <param name="action"></param>
        public void NotifyOnValueSet(Action<EGID, T> action)
        {
#if DEBUG && !PROFILE_SVELTO
            DBC.ECS.Check.Require(_subscriber == null, $"{this.GetType().Name}: listener already registered");
#endif
            _subscriber = action;
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

        protected T    _value;
        readonly  EGID _senderID;

        Action<EGID, T> _subscriber;
        bool            _paused;
    }
}