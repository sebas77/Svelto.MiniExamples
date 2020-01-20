using System;

namespace Svelto.ECS
{
    public class DispatchOnSet<T>
    {
        public DispatchOnSet(EGID senderID)
        {      
            _senderID = senderID;
        }
        
        public T value
        {
            set
            {
                _value = value;

                if (_paused == false)
                    _subscribers(_senderID, value);
            }
        }
        
        public void NotifyOnValueSet(Action<EGID, T> action)
        {
            _subscribers = action;
            _paused = false;
        }

        public void StopNotify()
        {
            _subscribers = null;
            _paused = true;
        }

        public void PauseNotify() { _paused = true; }
        public void ResumeNotify() { _paused = false; }

        protected T  _value;
        readonly EGID _senderID;

        Action<EGID, T> _subscribers;
        bool            _paused;
    }
}
