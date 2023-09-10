using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI
{
    public class GUIBlackboard
    {
        internal GUIBlackboard(GUIResources guiResources)
        {
            _guiResources          = guiResources;
            _widgetDataMap         = new Dictionary<string, WidgetDataSource>();
            _implementorToBindings = new Dictionary<IGUIImplementor, List<GUIDataBinding>>();
            _bindingsInfoByType    = new Dictionary<Type, TypeBinding>();
        }

        internal WidgetDataSource GetData(GUIComponent widget)
        {
            if (_widgetDataMap.TryGetValue(_guiResources.FromECS<string>(widget.root), out var data) == false)
            {
                data = EmptyDataSource;
            }

            return data;
        }

        internal void AddData(string key, WidgetDataSource data)
        {
            if (_widgetDataMap.ContainsKey(key))
            {
                _widgetDataMap[key].AddData(data._data);
            }
            else
            {
                _widgetDataMap[key] = data;
            }
        }

        internal void RemoveData(GUIComponent widget)
        {
            DBC.Check.Ensure(widget.isRoot);
            _widgetDataMap.Remove(_guiResources.FromECS<string>(widget.root));
        }

#region DataBinding
        internal void AddBindings(GUIComponent guiComponent, IGUIImplementor implementor)
        {
            var typeBinding = GetTypeBinding(implementor.GetType());
            var rootName    = _guiResources.FromECS<string>(guiComponent.root);
            foreach (var binding in typeBinding.bindings)
            {
                var callback = CreateBindingCallback(binding, implementor);

                var dataKey = (GUIDataKey)binding.fieldInfo.GetValue(implementor);
                if (dataKey)
                {
                    string targetTag = null;
                    if (GUITags.ExtractTag(dataKey.Value, out char tag, out string target))
                    {
                        if (tag == 'd')
                        {
                            targetTag = $"{rootName}.{target}";
                        }

                        if (tag == 'g')
                        {
                            targetTag = target;
                        }

                        if (tag == 'c')
                        {
                            GUITags.ParseFullname(_guiResources.FromECS<string>(guiComponent.container), out var containerName, out _);
                            targetTag = $"{containerName}.{target}";
                        }
                    }
                    // Anything without a tag is treated as a literal, so there is no binding needed.
                    // However we want to execute the callback it to allow engines to treat these literals.
                    else if (string.IsNullOrEmpty(target) == false &&
                             callback is DataBindingUpdate<string> stringCallback)
                    {
                        stringCallback(target);
                        continue;
                    }

                    targetTag ??= dataKey.Value;

                    AddBinding(targetTag, implementor, callback);
                }
            }
        }

        Delegate CreateBindingCallback(FieldBinding binding, IGUIImplementor implementor)
        {
            Delegate callback;
            if (binding.valueType == typeof(string))
            {
                callback = Delegate.CreateDelegate(typeof(DataBindingUpdate<string>), implementor, binding.callbackInfo, true);
            }
            else if (binding.valueType == typeof(int))
            {
                callback = Delegate.CreateDelegate(typeof(DataBindingUpdate<int>), implementor, binding.callbackInfo, true);
            }
            else if (binding.valueType == typeof(float))
            {
                callback = Delegate.CreateDelegate(typeof(DataBindingUpdate<float>), implementor, binding.callbackInfo, true);
            }
            else if (binding.valueType == typeof(bool))
            {
                callback = Delegate.CreateDelegate(typeof(DataBindingUpdate<bool>), implementor, binding.callbackInfo, true);
            }
            else
            {
                callback = Delegate.CreateDelegate(typeof(DataBindingUpdate<object>), implementor, binding.callbackInfo, true);
            }

            return callback;
        }

        void AddBinding(string tag, IGUIImplementor implementor, Delegate callback)
        {
            GetWidgetDataAndField(tag, out var widgetData, out var fieldKey);
            DBC.Check.Ensure(widgetData != null, $"Widget data source not found: {tag}");
            var binding = new GUIDataBinding
            {
                dataSource = widgetData,
                callback   = callback,
                key        = fieldKey
            };
            widgetData.AddBinding(binding);

            // Map implementors
            if (_implementorToBindings.TryGetValue(implementor, out var implementorBindings) == false)
            {
                implementorBindings                 = new List<GUIDataBinding>();
                _implementorToBindings[implementor] = implementorBindings;
            }

            implementorBindings.Add(binding);
        }

        internal void RemoveBindings(IGUIImplementor implementor)
        {
            if (_implementorToBindings.TryGetValue(implementor, out var bindings))
            {
                foreach (var binding in bindings)
                {
                    binding.dataSource.RemoveBinding(binding);
                }
            }
        }

        TypeBinding GetTypeBinding(Type target)
        {
            if (_bindingsInfoByType.TryGetValue(target, out TypeBinding typeBindings) == false)
            {
                var boundFields = new List<FieldBinding>();
                var fields =
                    target.GetFields(BindableFlags);
                foreach (var field in fields)
                {
                    var bindable = (BindableAttribute)Attribute.GetCustomAttribute(field, typeof(BindableAttribute));
                    if (field.FieldType != typeof(GUIDataKey) || bindable == null) continue;

                    var callbackInfo = target.GetMethod(bindable.callbackName, BindableFlags);
                    var callbackParameters = callbackInfo.GetParameters();

                    if (callbackParameters.Length == 1 && callbackParameters[0].ParameterType == bindable.callbackValueType)
                    {
                        boundFields.Add(new FieldBinding
                        {
                            fieldInfo    = field,
                            callbackInfo = callbackInfo,
                            valueType    = bindable.callbackValueType
                        });
                    }
                }

                typeBindings.bindings       = boundFields.ToArray();
                _bindingsInfoByType[target] = typeBindings;
            }

            return typeBindings;
        }

        void GetWidgetDataAndField(string key, out WidgetDataSource data, out string fieldKey)
        {
            var splitPoint = key.IndexOf('.');
            DBC.Check.Ensure(splitPoint >= 0 && splitPoint < key.Length - 1, $"Binding field has the wrong format: {key}");

            var dataSourceKey = key.Substring(0, splitPoint);
            fieldKey = key.Substring(splitPoint + 1);
            DBC.Check.Ensure(fieldKey.IndexOf('.') < 0, $"Binding field has the wrong format: {key}");

            if (_widgetDataMap.TryGetValue(dataSourceKey, out data) == false)
            {
                data                          = new WidgetDataSource();
                _widgetDataMap[dataSourceKey] = data;
            }

            _widgetDataMap.TryGetValue(dataSourceKey, out data);
        }

#endregion

        readonly GUIResources                                      _guiResources;
        readonly Dictionary<string, WidgetDataSource>              _widgetDataMap;
        readonly Dictionary<IGUIImplementor, List<GUIDataBinding>> _implementorToBindings;
        readonly Dictionary<Type, TypeBinding>                     _bindingsInfoByType;

        struct FieldBinding
        {
            public FieldInfo  fieldInfo;
            public MethodInfo callbackInfo;
            public Type       valueType;
        }

        struct TypeBinding
        {
            public FieldBinding[] bindings;
        }

        static readonly WidgetDataSource EmptyDataSource = new WidgetDataSource();
        const BindingFlags BindableFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    }
}