using System;
using Svelto.WeakEvents;

namespace Svelto.ECS
{
    public class DispatchOnSet<T> where T:struct
    {
        static ExclusiveGroup OBSOLETE_GROUP = new ExclusiveGroup();
        
        public DispatchOnSet(EGID senderID)
        {      
            _subscribers = new WeakEvent<EGID, T>();
            
            _senderID = senderID;
        }
        
        public T value
        {
            set
            {
                _value = value;

                _subscribers.Invoke(_senderID, value);
            }

            get => _value;
        }
        
        public void NotifyOnValueSet(Action<EGID, T> action)
        {
            _subscribers += action;    
        }

        public void StopNotify(Action<EGID, T> action)
        {
            _subscribers -= action;
        }

        protected T  _value;
        readonly EGID _senderID;

        WeakEvent<EGID, T> _subscribers;
    }
}
