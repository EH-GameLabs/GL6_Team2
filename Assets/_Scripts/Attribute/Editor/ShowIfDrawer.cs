using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty boolProp = property.serializedObject.FindProperty(showIf.BoolName);

        if (boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean)
        {
            if (boolProp.boolValue == showIf.ExpectedValue)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label, true); // fallback se bool non trovato
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty boolProp = property.serializedObject.FindProperty(showIf.BoolName);

        if (boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean)
        {
            if (boolProp.boolValue == showIf.ExpectedValue)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                return 0f;
            }
        }

        return EditorGUI.GetPropertyHeight(property, label, true); // fallback
    }
}
