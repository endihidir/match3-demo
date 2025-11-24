using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Attributes;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(ConstantDropdown))]
    public class ConstantDropdownDrawer : PropertyDrawer
    {
        private string[] GetConstants(Type type)
        {
            var constants = new List<string>();

            if (type == null)
            {
                Debug.LogError("Target type is null.");
                return constants.ToArray();
            }

            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo fi in fieldInfos)
            {
                if (fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                {
                    constants.Add((string)fi.GetRawConstantValue());
                }
            }

            return constants.ToArray();
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            if (property.propertyType == SerializedPropertyType.String)
            {
                ConstantDropdown attr = (ConstantDropdown)attribute;
                var constants = GetConstants(attr.TargetType);

                if (constants == null || constants.Length == 0)
                {
                    EditorGUI.LabelField(rect, label.text, "No constants found in target type");
                    return;
                }

                string propertyString = property.stringValue;
                int index = 0;

                for (int i = 0; i < constants.Length; i++)
                {
                    if (constants[i].Equals(propertyString, StringComparison.Ordinal))
                    {
                        index = i;
                        break;
                    }
                }

                int newIndex = EditorGUI.Popup(rect, label.text, index, constants);
                string newValue = newIndex >= 0 ? constants[newIndex] : string.Empty;

                if (!property.stringValue.Equals(newValue, StringComparison.Ordinal))
                {
                    property.stringValue = newValue;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, includeChildren: true);
        }
    }
}
