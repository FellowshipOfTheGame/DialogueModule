using UnityEditor;
using UnityEngine;

namespace Fog.Dialogue {
    [CustomEditor(typeof(DialogueScrollPanel))]
    public class DialogueScrollPanelEditor : UnityEditor.UI.ScrollRectEditor {
        [SerializeField] private bool wasVerticalLast = true;

        public override void OnInspectorGUI() {
            EditorGUILayout.LabelField("Custom Scroll Rect Fields", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothScrolling"), new GUIContent("Smooth Scrolling"));
            SerializedProperty prop = serializedObject.FindProperty("smoothScrolling");
            if (prop != null && prop.propertyType == SerializedPropertyType.Boolean) {
                if (prop.boolValue) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollSpeed"), new GUIContent("Scroll Speed"));
                }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollUpIndicator"), new GUIContent("Scroll Up Indicator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollDownIndicator"), new GUIContent("Scroll Down Indicator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skipIndicator"), new GUIContent("Skip Indicator"));
            // Trying to restrict to only one type of scrolling
            // Cant find serialized property of parent class, so doesnt work
            // SerializedProperty verticalProp = serializedObject.FindProperty("vertical");
            // SerializedProperty horizontalProp = serializedObject.FindProperty("horizontal");
            // if(verticalProp != null && horizontalProp != null){
            //     if(verticalProp.boolValue && horizontalProp.boolValue){
            //         if(wasVerticalLast){
            //             wasVerticalLast = false;
            //             verticalProp.boolValue = false;
            //         }else{
            //             wasVerticalLast = true;
            //             horizontalProp.boolValue = false;
            //         }
            //     }
            // }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Regular Scroll Rect Fields", EditorStyles.boldLabel);
            base.OnInspectorGUI();
        }
    }
}
