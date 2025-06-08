using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginDisabledGroup(true);

        // Rende espandibili anche oggetti complessi come classi e liste
        EditorGUI.PropertyField(position, property, label, true);

        EditorGUI.EndDisabledGroup();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Assicura il layout corretto anche con oggetti complessi
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
