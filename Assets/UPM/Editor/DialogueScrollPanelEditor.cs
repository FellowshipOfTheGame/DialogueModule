using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Fog.Dialogue {
    [CustomEditor(typeof(DialogueScrollPanel))]
    public class DialogueScrollPanelEditor : ScrollRectEditor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.LabelField("Custom Scroll Rect Fields", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothScrolling"),
                                          new GUIContent("Smooth Scrolling"));
            SerializedProperty prop = serializedObject.FindProperty("smoothScrolling");
            if (prop != null && prop.propertyType == SerializedPropertyType.Boolean) {
                if (prop.boolValue) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollSpeed"),
                                                  new GUIContent("Scroll Speed"));
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollUpIndicator"),
                                          new GUIContent("Scroll Up Indicator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollDownIndicator"),
                                          new GUIContent("Scroll Down Indicator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skipIndicator"),
                                          new GUIContent("Skip Indicator"));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Regular Scroll Rect Fields", EditorStyles.boldLabel);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}