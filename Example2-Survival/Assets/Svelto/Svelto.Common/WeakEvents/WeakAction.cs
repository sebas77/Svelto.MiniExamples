using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Svelto.WeakEvents
{
    public class WeakAction<T1, T2> : WeakActionBase
    {
        public WeakAction(Action<T1, T2> listener)
            : base(listener.Target, listener.GetMethodInfoEx())
        {}

        public bool Invoke(T1 data1, T2 data2)
        {
            _data[0] = data1;
            _data[1] = data2;

            return Invoke_Internal(_data);
        }

        readonly object[] _data = new object[2];
    }

    public class WeakAction<T> : WeakActionBase
    {
        public WeakAction(Action<T> listener)
            : base(listener.Target, listener.GetMethodInfoEx())
        {}

        public bool Invoke(T data)
        {
            _data[0] = data;

            return Invoke_Internal(_data);
        }

        readonly object[] _data = new object[1];
    }

    public class WeakAction : WeakActionBase
    {
        public WeakAction(Action listener) : base(listener)
        {}

        public WeakAction(object listener, MethodInfo method) : base(listener, method)
        {}

        public bool Invoke()
        {
            return Invoke_Internal(null);
        }
    }

    public abstract class WeakActionBase
    {
        readonly DataStructures.WeakReference<object> objectRef;
        readonly MethodInfo method;

        public bool IsValid
        {
            get { return objectRef.IsValid; }
        }

        protected WeakActionBase(Action listener)
            : this(listener.Target, listener.GetMethodInfoEx())
        {}

        protected WeakActionBase(object listener, MethodInfo method)
        {
            objectRef = new DataStructures.WeakReference<object>(listener);

            this.method = method;
            
            if (method.IsStatic == true)
                throw new ArgumentException("Cannot create weak event to a static method");

#if NETFX_CORE 
            var attributes = (CompilerGeneratedAttribute[])method.GetType().GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
            if (attributes.Length != 0)
                throw new ArgumentException("Cannot create weak event to anonymous method with closure.");
#else
            if (method.DeclaringType.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length != 0)
                throw new ArgumentException("Cannot create weak event to anonymous method with closure.");
#endif
        }

        protected bool Invoke_Internal(object[] data)
        {
            //please do not add the try catch here, it's very annoying to not be able to check the real stack
            if (objectRef.IsValid)
            {
                method.Invoke(objectRef.Target, data);

                return true;
            }
            
            Console.LogError("<color=orange>Svelto.Common.WeakAction</color> Target of weak action has been garbage collected");

            return false;
        }
        
        public bool IsMatch(object thisObject, MethodInfo thisMethod)
        {
            return objectRef.Target == thisObject && method == thisMethod;
        }
    }
}