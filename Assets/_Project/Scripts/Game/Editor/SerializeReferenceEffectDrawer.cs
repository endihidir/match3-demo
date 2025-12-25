using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Core.Config.BoosterActionBase), true)]
public sealed class BoosterEffectBaseDrawer : PropertyDrawer
{
    private static Type[] _cachedTypes;

    private static Type[] GetConcreteTypes()
    {
        if (_cachedTypes != null) return _cachedTypes;

        _cachedTypes = TypeCache.GetTypesDerivedFrom<Core.Config.BoosterActionBase>()
            .Where(t => !t.IsAbstract && !t.IsGenericType && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name)
            .ToArray();

        return _cachedTypes;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var h = EditorGUIUtility.singleLineHeight;

        if (property.managedReferenceValue == null)
            return h;

        if (!property.isExpanded)
            return h;

        h += EditorGUIUtility.standardVerticalSpacing;

        var it = property.Copy();
        var end = it.GetEndProperty();
        var enterChildren = true;

        while (it.NextVisible(enterChildren) && !SerializedProperty.EqualContents(it, end))
        {
            enterChildren = false;
            h += EditorGUI.GetPropertyHeight(it, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        if (property.managedReferenceValue == null)
        {
            DrawNullRow(row, property);
            EditorGUI.EndProperty();
            return;
        }

        DrawHeaderRow(row, property);

        if (property.isExpanded)
        {
            var y = row.yMax + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel++;

            var it = property.Copy();
            var end = it.GetEndProperty();
            var enterChildren = true;

            while (it.NextVisible(enterChildren) && !SerializedProperty.EqualContents(it, end))
            {
                enterChildren = false;

                var h = EditorGUI.GetPropertyHeight(it, true);
                var r = new Rect(position.x, y, position.width, h);

                EditorGUI.PropertyField(r, it, true);

                y += h + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private static void DrawNullRow(Rect row, SerializedProperty property)
    {
        var btn = row;
        btn.width -= 90f;

        if (GUI.Button(btn, "Add Effect"))
            ShowCreateMenu(property);

        var clr = row;
        clr.xMin = btn.xMax + 4f;

        if (GUI.Button(clr, "Clear"))
        {
            property.serializedObject.Update();
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private static void DrawHeaderRow(Rect row, SerializedProperty property)
    {
        var typeName = property.managedReferenceValue.GetType().Name;

        var fold = row;
        fold.width -= 90f;

        property.isExpanded = EditorGUI.Foldout(fold, property.isExpanded, typeName, true);

        var menu = row;
        menu.xMin = fold.xMax + 4f;
        menu.width = 86f;

        if (GUI.Button(menu, "Change"))
            ShowCreateMenu(property, keepExpanded: true);

        var remove = row;
        remove.xMin = menu.xMax + 4f;
        remove.xMax = row.xMax;

        if (GUI.Button(remove, "X"))
        {
            property.serializedObject.Update();
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private static void ShowCreateMenu(SerializedProperty property, bool keepExpanded = false)
    {
        var menu = new GenericMenu();
        var types = GetConcreteTypes();

        for (int i = 0; i < types.Length; i++)
        {
            var t = types[i];
            menu.AddItem(new GUIContent(t.Name), false, () =>
            {
                property.serializedObject.Update();

                property.managedReferenceValue = Activator.CreateInstance(t);

                if (keepExpanded)
                    property.isExpanded = true;

                property.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }
}