using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UpgradeDisplayCondition))]
public class UpgradeDisplayConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var conditionTypeProp = property.FindPropertyRelative("conditionType");
        var requiredCountProp = property.FindPropertyRelative("requiredOwnedCount");
        var requiredGeneratorProp = property.FindPropertyRelative("RequiredGenerator");

        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight + 2;
        float y = position.y;

        var typeRect = new Rect(position.x, y, position.width, lineHeight);
        EditorGUI.PropertyField(typeRect, conditionTypeProp);
        y += lineHeight;

        if ((DisplayConditionType)conditionTypeProp.enumValueIndex == DisplayConditionType.GeneratorOwnedCount)
        {
            var genRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.PropertyField(genRect, requiredGeneratorProp, new GUIContent("Required Generator"));
            y += lineHeight;

            var countRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.PropertyField(countRect, requiredCountProp, new GUIContent("Required Count"));
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var conditionTypeProp = property.FindPropertyRelative("conditionType");

        if ((DisplayConditionType)conditionTypeProp.enumValueIndex == DisplayConditionType.GeneratorOwnedCount)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 6;
        }

        return EditorGUIUtility.singleLineHeight + 2;
    }
}
