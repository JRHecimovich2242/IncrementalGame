using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpgradeData))]
public class UpgradeDataEditor : Editor
{
    private SerializedProperty _upgradeNameProp;
    private SerializedProperty _upgradeCostProperty;
    private SerializedProperty _upgradeCostTypeProperty;
    private SerializedProperty _descriptionProp;
    private SerializedProperty _targetGeneratorProp;
    private SerializedProperty _upgradeTypeProp;
    private SerializedProperty _upgradeValueProp;
    private SerializedProperty _displayConditionProp;
    private SerializedProperty _upgradeVisualsProp;

    private void OnEnable()
    {
        _upgradeNameProp = serializedObject.FindProperty("UpgradeName");
        _descriptionProp = serializedObject.FindProperty("UpgradeDescription");
        _upgradeCostProperty = serializedObject.FindProperty("Cost");
        _upgradeCostTypeProperty = serializedObject.FindProperty("CostType");
        _targetGeneratorProp = serializedObject.FindProperty("TargetGenerator");
        _upgradeTypeProp = serializedObject.FindProperty("Type");
        _upgradeValueProp = serializedObject.FindProperty("UpgradeValue");
        _displayConditionProp = serializedObject.FindProperty("DisplayCondition");
        _upgradeVisualsProp = serializedObject.FindProperty("UpgradeVisuals");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_upgradeNameProp);
        EditorGUILayout.PropertyField(_descriptionProp);
        EditorGUILayout.PropertyField(_upgradeCostProperty);
        EditorGUILayout.PropertyField(_upgradeCostTypeProperty);
        EditorGUILayout.PropertyField(_upgradeTypeProp);
        EditorGUILayout.PropertyField(_upgradeValueProp);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Target Generator", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_targetGeneratorProp);

        if (_targetGeneratorProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("This upgrade has no target generator assigned!", MessageType.Warning);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Display Condition", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(_displayConditionProp);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(_upgradeVisualsProp);

        serializedObject.ApplyModifiedProperties();
    }
}
