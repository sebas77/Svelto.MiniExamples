using Svelto.DataStructures;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Svelto.WeakEvents
{
    public class WeakEvent
    {
        public static WeakEvent operator+(WeakEvent c1, Action x)
        {
            if (c1 == null) c1 = new WeakEvent();
            c1.Add(x);

            return c1;
        }

        public static WeakEvent operator-(WeakEvent c1, Action x)
        {
            DBC.Common.Check.Require(x != null);
            c1.Remove(x);

            return c1;
        }

        public void Add(Action x)
        {
            _subscribers.Add(new WeakAction(x));
        }

        public void Remove(Action x)
        {
            RemoveInternal(x.Target, x.GetMethodInfoEx());
        }

        public void Invoke()
        {
            InternalInvoke(_invoke);
        }
        
        protected virtual bool InvokeDelegate(WeakActionBase action)
        {
            return ((WeakAction) action).Invoke();
        }

        void InternalInvoke(System.Func<WeakActionBase, bool> invoke)
        {
            _isIterating = true;

            for (int i = 0; i < _subscribers.Count; i++)
                if (invoke(_subscribers[i]) == false)
                    _subscribers.UnorderedRemoveAt(i--);
            
            _isIterating = false;

            for (int i = 0; i < _toRemove.Count; i++)
                RemoveInternal(_toRemove[i].Key, _toRemove[i].Value);
            
            _toRemove.Clear();
        }

        protected void RemoveInternal(object thisObject, MethodInfo thisMethod)
        {
            if (_isIterating == false)
            {
                for (int i = 0; i < _subscribers.Count; ++i)
                {
                    var otherObject = _subscribers[i];

                    if (otherObject.IsMatch(thisObject, thisMethod))
                    {
                        _subscribers.UnorderedRemoveAt(i);
                        
                        return;
                    }
                }
            }
            else
            {
                _toRemove.Add(new KeyValuePair<object, MethodInfo>(thisObject, thisMethod));
            }
        }
        
        public WeakEvent()
        {
            _invoke = InvokeDelegate;
        }
        
        public void Clear()
        {
            _subscribers.Clear();
        }

        protected readonly FasterList<WeakActionBase> _subscribers = new FasterList<WeakActionBase>();
        readonly FasterList<KeyValuePair<object, MethodInfo>>
            _toRemove = new FasterList<KeyValuePair<object, MethodInfo>>();

        bool _isIterating;
        Func<WeakActionBase, bool> _invoke;
    }

    public class WeakEvent<T1>:WeakEvent
    {
        T1 _arg;

        public static WeakEvent<T1> operator+(WeakEvent<T1> c1, Action<T1> x)
        {
            if (c1 == null) c1 = new WeakEvent<T1>();
            c1.Add(x);

            return c1;
        }

        public static WeakEvent<T1> operator-(WeakEvent<T1> c1, Action<T1> x)
        {
            DBC.Common.Check.Require(x != null);
            c1.Remove(x);

            return c1;
        }

        public void Add(Action<T1> x)
        {
            _subscribers.Add(new WeakAction<T1>(x));
        }

        public void Remove(Action<T1> x)
        {
            RemoveInternal(x.Target, x.GetMethodInfoEx());
        }
        
        public void Invoke(T1 arg)
        {
            _arg = arg;
            
            base.Invoke();
        }

        protected override bool InvokeDelegate(WeakActionBase action)
        {
            return ((WeakAction<T1>) action).Invoke(_arg);
        }
    }

    public class WeakEvent<T1, T2>:WeakEvent
    {
        T1 _arg1;
        T2 _arg2;

        public static WeakEvent<T1, T2> operator+(WeakEvent<T1, T2> c1, Action<T1, T2> x)
        {
            if (c1 == null) c1 = new WeakEvent<T1, T2>();
            c1._subscribers.Add(new WeakAction<T1, T2>(x));

            return c1;
        }

        public static WeakEvent<T1, T2> operator-(WeakEvent<T1, T2> c1, Action<T1, T2> x)
        {
            DBC.Common.Check.Require(x != null);
            c1.Remove(x);

            return c1;
        }

        public void Add(Action<T1, T2> x)
        {
            _subscribers.Add(new WeakAction<T1, T2>(x));
        }

        public void Remove(Action<T1, T2> x)
        {
            RemoveInternal(x.Target, x.GetMethodInfoEx());
        }

        public void Invoke(T1 arg1, T2 arg2)
        {
            _arg1 = arg1;
            _arg2 = arg2;
            
            base.Invoke();
        }

        protected override bool InvokeDelegate(WeakActionBase action)
        {
            return ((WeakAction<T1, T2>) action).Invoke(_arg1, _arg2);
        }
    }
}
