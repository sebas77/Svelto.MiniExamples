using System;

namespace Svelto.ECS.GUI
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BindableAttribute : Attribute
    {
        public string callbackName { get; }
        public Type   callbackValueType { get; }

        public BindableAttribute(string name, Type type = null)
        {
            callbackValueType = type ?? typeof(object);
            callbackName = name;
        }
    }
}