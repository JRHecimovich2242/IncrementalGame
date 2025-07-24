using UnityEditor;
using UnityEngine;
using System.Numerics;

[CustomPropertyDrawer(typeof(HugeInt))]
[CanEditMultipleObjects]
public class HugeIntDrawer : PropertyDrawer
{
    // Optional: key to store foldout/toggle state per property path
    private const string PreviewPrefKey = "HugeIntDrawer_ShowSci_";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get backing string
        var strProp = property.FindPropertyRelative("_serialized");
        if (strProp == null)
        {
            EditorGUI.LabelField(position, label.text, "Missing _serialized field");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        // Layout:
        // [Label][TextField][gear icon / toggle]
        var fullRect = position;

        // Reserve a small rect for the preview toggle button
        const float btnW = 22f;
        var fieldRect = new Rect(fullRect.x, fullRect.y, fullRect.width - btnW - 2, EditorGUIUtility.singleLineHeight);
        var btnRect = new Rect(fieldRect.xMax + 2, fieldRect.y, btnW, fieldRect.height);

        // Show mixed value dash if different across selections
        EditorGUI.showMixedValue = strProp.hasMultipleDifferentValues;

        // DelayedTextField commits on enter/tab or focus loss.
        string newString = EditorGUI.DelayedTextField(fieldRect, label, strProp.stringValue);

        EditorGUI.showMixedValue = false;

        // Toggle scientific preview
        bool showSci = EditorPrefs.GetBool(PreviewPrefKey + property.propertyPath, false);
        if (GUI.Button(btnRect, showSci ? "E" : "123", EditorStyles.miniButton))
        {
            showSci = !showSci;
            EditorPrefs.SetBool(PreviewPrefKey + property.propertyPath, showSci);
        }

        // Draw scientific preview below (optional)
        if (showSci && !strProp.hasMultipleDifferentValues && !string.IsNullOrEmpty(strProp.stringValue))
        {
            TryParse(strProp.stringValue, out var currentBig);
            string sci = ToScientific(currentBig, 3);

            var sciRect = new Rect(fullRect.x, fullRect.y + EditorGUIUtility.singleLineHeight + 2,
                                   fullRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(sciRect, $"≈ {sci}", EditorStyles.miniLabel);
        }

        // Apply changes if parse succeeds
        if (EditorGUI.EndChangeCheck())
        {
            if (TryParse(newString, out _))
            {
                strProp.stringValue = newString;
            }
            else
            {
                // Show a one-frame warning (optional) or log
                Debug.LogWarning($"HugeInt parse failed for: \"{newString}\"");
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        bool showSci = EditorPrefs.GetBool(PreviewPrefKey + property.propertyPath, false);
        return showSci
            ? EditorGUIUtility.singleLineHeight * 2 + 2
            : EditorGUIUtility.singleLineHeight;
    }

    private static bool TryParse(string s, out BigInteger result)
    {
        // accept empty as zero
        if (string.IsNullOrWhiteSpace(s))
        {
            result = BigInteger.Zero;
            return true;
        }
        return BigInteger.TryParse(s, out result);
    }

    private static string ToScientific(BigInteger value, int sigDigits)
    {
        if (value.IsZero) return "0";
        var s = value.ToString();
        int exp = s.Length - 1;
        string mantissa = s[0] +
                          (sigDigits > 1 && s.Length > 1
                           ? "." + s.Substring(1, Mathf.Min(sigDigits - 1, s.Length - 1))
                           : "");
        return $"{mantissa}e{exp}";
    }
}
