using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
        GUI.enabled = false;

        property.serializedObject.forceChildVisibility = true;
        EditorGUI.PropertyField(position, property, label, true);
        property.serializedObject.forceChildVisibility = false;
        
        GUI.enabled = true;


    }
}
