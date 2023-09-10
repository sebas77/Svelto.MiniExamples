using System;
using UnityEngine;

namespace Svelto.ECS.GUI
{
    [Serializable]
    public class GUIDataKey
    {
        [SerializeField] string _value;
        public string Value => string.IsNullOrEmpty(_value) ? null : _value;

        public static implicit operator bool(GUIDataKey key)
        {
            return key != null && string.IsNullOrEmpty(key._value) == false;
        }
    }
}