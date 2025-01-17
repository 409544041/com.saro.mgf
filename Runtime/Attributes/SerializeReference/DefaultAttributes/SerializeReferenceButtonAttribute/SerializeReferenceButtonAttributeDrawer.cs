﻿#if UNITY_EDITOR && UNITY_2019_4_OR_NEWER && !ODIN_INSPECTOR

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializeReferenceButtonAttribute))]
public class SerializeReferenceButtonAttributeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, property.isExpanded);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //EditorGUI.BeginProperty(position, label, property);

        //var labelPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        //EditorGUI.LabelField(labelPosition, label);

        var typeRestrictions = SerializedReferenceUIDefaultTypeRestrictions.GetAllBuiltInTypeRestrictions(fieldInfo);
        property.DrawSelectionButtonForManagedReference(position, typeRestrictions);

        EditorGUI.PropertyField(position, property, label, true);
        //EditorGUI.PropertyField(position, property, GUIContent.none, true);

        //EditorGUI.EndProperty();
    }
}
#endif