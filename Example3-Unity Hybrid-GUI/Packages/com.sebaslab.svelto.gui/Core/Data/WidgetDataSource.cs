using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.ECS.GUI.DBC;

namespace Svelto.ECS.GUI
{
    /**
     * This class represents the data source of a widget hierarchy, this is strictly so by creating only one of these
     * per root widget. This data is accessible to widgets through data binding and command parameters.
     *
     * NOTE: This data is also passed to the GUI Builder but this might be a behaviour that might not be needed anymore.
     *
     * For each instance we hold:
     *   - Data: A dictionary holding values keyed by a string. This dictionary can get updated through commands which
     *           would trigger data bindings to be updated accordingly. The strategy to update data sources is to merge
     *           keys by overriding old values with new values.
     *
     *   - Bindings:
     *           A list of bindings keyed by field name. This bindings hold callback functions that will update specific
     *           widget instances.
     *
     */
    public class WidgetDataSource
    {
        internal WidgetDataSource()
        {
            // TODO: To remove boxing we need to make this object become a StructValue.
            _data = new Dictionary<string, object>();
            _bindings = new Dictionary<string, List<GUIDataBinding>>();
        }

        public WidgetDataSource(object o)
        {
            if (o is Dictionary<string, object> data)
            {
                _data = data;
            }
            _bindings = new Dictionary<string, List<GUIDataBinding>>();
        }

        public WidgetDataSource(Dictionary<string, object> data)
        {
            _data = data;
            _bindings = new Dictionary<string, List<GUIDataBinding>>();
        }

        public bool IsEmpty => _data.Count == 0;

        public T[] GetArray<T>(string key)
        {
            Check.Assert(_data.ContainsKey(key), $"Gui params key:{key} not found");
            Check.Assert(_data[key] is List<object>, $"GuiParams: Expected array under key:{key} to be an array");
            return CastArray<T>(_data[key] as List<object>);
        }

        public T GetValue<T>(string key)
        {
            Check.Assert(_data.ContainsKey(key), $"Gui params key:{key} not found");
            return CastValue<T>(_data[key]);
        }

        internal T GetValueOrDefault<T>(string key, T defaultValue)
        {
            return _data.ContainsKey(key) ? CastValue<T>(_data[key]) : defaultValue;
        }

        public bool TryGetArray<T>(string key, out T[] values)
        {
            Check.Assert(!_data.ContainsKey(key) || _data[key] is List<object>
                       , $"GuiParams: Expected array under key:{key} to be an array");
            var keyFound = _data.TryGetValue(key, out var rawValue);
            values = keyFound ? CastArray<T>(rawValue as List<object>) : default;
            return keyFound;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var keyFound = _data.TryGetValue(key, out var rawValue);
            //value = keyFound ? CastValue<T>(rawValue) : default;
            //return keyFound;
            if (keyFound && rawValue is T castedValue)
            {
                value = castedValue;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        // NOTE: We are not supporting arrays of arrays currently.
        T[] CastArray<T>(List<object> values)
        {
            var results = new T[values.Count];
            for (var i = 0; i < values.Count; i++)
                results[i] = CastValue<T>(values[i]);
            return results;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T CastValue<T>(object value)
        {
            T result;
            if (value is Dictionary<string, object>)
            {
                var parameters = new WidgetDataSource((Dictionary<string, object>) value);
                result = (T) (object) parameters;
            }
            else
            {
                result = (T) value;
            }

            return result;
        }

        #region DataBinding
        public void AddData(Dictionary<string, object> other)
        {
            foreach (var kvp in other)
            {
                _data[kvp.Key] = kvp.Value;

                if (_bindings.TryGetValue(kvp.Key, out var bindings))
                {
                    foreach (var binding in bindings)
                    {
                        ExecuteBinding(binding, GetValueOrDefault(kvp.Key, default(object)));
                    }
                }
            }
        }
        public void AddData(string key, object value)
        {
            _data[key] = value;
            if (_bindings.TryGetValue(key, out var bindings))
            {
                foreach (var binding in bindings)
                {
                    ExecuteBinding(binding, GetValueOrDefault(binding.key, default(object)));
                }
            }
        }

        internal void AddBinding(GUIDataBinding binding)
        {
            if (_bindings.TryGetValue(binding.key, out var fieldBindings) == false)
            {
                fieldBindings = new List<GUIDataBinding>();
                _bindings[binding.key] = fieldBindings;
            }
            fieldBindings.Add(binding);

            // Execute callback immediately to get the starting values set.
            ExecuteBinding(binding, GetValueOrDefault(binding.key, default(object)));
        }

        internal void RemoveBinding(GUIDataBinding binding)
        {
            if (_bindings.TryGetValue(binding.key, out var fieldBindings))
            {
                fieldBindings.Remove(binding);
            }
        }

        internal void ExecuteBinding(GUIDataBinding binding, object parameter)
        {
            if (binding.callback is DataBindingUpdate<string> stringCallback)
            {
                stringCallback((string)parameter);
            }
            else if (binding.callback is DataBindingUpdate<int> intCallback)
            {
                intCallback((int) (parameter ?? default(int)));
            }
            else if (binding.callback is DataBindingUpdate<float> floatCallback)
            {
                floatCallback((float) (parameter ?? default(float)));
            }
            else if (binding.callback is DataBindingUpdate<bool> boolCallback)
            {
                boolCallback((bool) (parameter ?? default(bool)));
            }
            else
            {
                (binding.callback as DataBindingUpdate<object>)(parameter);
            }
        }
        #endregion


        internal Dictionary<string, object>                        _data;
        readonly Dictionary<string, List<GUIDataBinding>>          _bindings;
    }
}