#if UNITY_EDITOR
using System;
using System.Linq;
using Svelto.ECS.GUI.Commands;
using UnityEditor;
using UnityEngine;

namespace Svelto.ECS.GUI.Extensions.Unity.Editor
{
    [CustomPropertyDrawer(typeof(SerializedCommandData))]
    public class SerializedCommandDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.height = EditorGUIUtility.singleLineHeight;
            _height = 0;

            // Command Name Field
            SerializedProperty commandProperty = property.FindPropertyRelative("commandName");
            int selectedCommand = 0;
            for (var i = 1; i < commandNames.Length; i++)
            {
                if (commandNames[i] == commandProperty.stringValue)
                {
                    selectedCommand = i;
                    break;
                }
            }
            selectedCommand = EditorGUI.Popup(position, selectedCommand, commandNames);
            commandProperty.stringValue = selectedCommand > 0 ? commandNames[selectedCommand] : "";
            Advance(ref position, EditorGUIUtility.singleLineHeight);

            if (selectedCommand > 0)
            {
                // Other fields.
                PropertyField(ref position, property.FindPropertyRelative("target"));
                PropertyField(ref position, property.FindPropertyRelative("parameter1"));
                PropertyField(ref position, property.FindPropertyRelative("parameter2"));
                PropertyField(ref position, property.FindPropertyRelative("parameter3"));
            }

            /*
            if (_commandName != null)
            {
                commandProperty.stringValue = _commandName;
                _commandName = null;
            }
            */

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _height + EditorGUIUtility.singleLineHeight;
        }

        void Advance(ref Rect position, float lineHeight)
        {
            lineHeight += EditorGUIUtility.singleLineHeight * 0.1f;
            _height += lineHeight;
            position.y += lineHeight;
            // Reset rect height by default.
            position.height = EditorGUIUtility.singleLineHeight;
        }

        void PropertyField(ref Rect position, SerializedProperty property)
        {
            position.height = EditorGUI.GetPropertyHeight(property);
            EditorGUI.PropertyField(position, property);
            Advance(ref position, position.height);
        }

        float _height;
        string _commandName;

        static SerializedCommandDataPropertyDrawer()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            commandNames = assemblies.AsParallel().SelectMany(a => a.GetTypes())
                .Where(t => typeof(GUICommand).IsAssignableFrom(t))
                .Select(t => (GUICommandAttribute)Attribute.GetCustomAttribute(t, typeof(GUICommandAttribute)))
                .Where(attr => attr != null)
                .OrderBy(attr => attr.commandName)
                .Select(attr => attr.commandName)
                .Prepend("None").ToArray();
        }

        static readonly string[] commandNames;
    }
}
#endif