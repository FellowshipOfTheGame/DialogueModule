using UnityEngine;
using UnityEditor;

namespace Fog.Dialogue.Editor {
    [CustomPropertyDrawer(typeof(HideInInspectorIf))]
    [CustomPropertyDrawer(typeof(HideInInspectorIfNot))]
    public class HideInInspectorIfDrawer : PropertyDrawer
    {
        private string ConditionName => ((BaseHideInInspectorIf)attribute).conditionName;
        private bool InvertCondition => ((BaseHideInInspectorIf)attribute).invertCondition;
        private bool IsHidden(SerializedProperty property) {
            SerializedProperty condition = property.serializedObject.FindProperty(property.propertyPath.Replace(property.name, ConditionName));
            if (condition != null) {
                return InvertCondition ? !condition.boolValue : condition.boolValue;
            } else {
                Debug.LogWarning($"{property.GetType()}.{property.name}: Can't find field with name: {ConditionName}");
                return false;
            }
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!IsHidden(property)) {
                EditorGUI.BeginProperty(position, label, property);
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
                EditorGUI.EndProperty();
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return IsHidden(property) ? -EditorGUIUtility.standardVerticalSpacing : EditorGUI.GetPropertyHeight(property, label);
        }
    }
}