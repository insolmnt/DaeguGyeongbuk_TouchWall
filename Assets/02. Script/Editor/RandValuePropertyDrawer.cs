using StartPage;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RandValue))]
public class RandValuePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        //label.text = "";


        Rect contentPosition = EditorGUI.PrefixLabel(position, label);
        float width = contentPosition.width;
        contentPosition.width = contentPosition.width * 1.2f;
        EditorGUI.indentLevel = 0;


        contentPosition.x = 200;
        contentPosition.width = width * 0.30f;
        EditorGUIUtility.labelWidth = 30f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("min"), new GUIContent("Min"));


        contentPosition.x += contentPosition.width + 40;
        contentPosition.width = width * 0.30f;
        EditorGUIUtility.labelWidth = 30f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("max"), new GUIContent("Max"));

        EditorGUI.EndProperty();
    }
}
