using UnityEngine;
using System.Collections;
using UnityEditor;
namespace WingroveAudio
{
    [CustomEditor(typeof(EventReceiveAction))]
    public class EventReceiveActionEditor : Editor
    {
        int bankIndex = -1;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty eventProperty = serializedObject.FindProperty("m_event");
            bankIndex = EditorUtilities.EventProperty(bankIndex, eventProperty);

            SerializedProperty actionProperty = serializedObject.FindProperty("m_action");
            EditorGUILayout.PropertyField(actionProperty);
            SerializedProperty fadeProperty = serializedObject.FindProperty("m_fadeLength");
            EditorGUILayout.PropertyField(fadeProperty);
            SerializedProperty delayProperty = serializedObject.FindProperty("m_delay");
            EditorGUILayout.PropertyField(delayProperty);
            if (actionProperty.enumValueIndex == (int)EventReceiveAction.Actions.PlayRandomNoRepeats)
            {
                SerializedProperty repeatsProperty = serializedObject.FindProperty("m_noRepeatsMemory");
                EditorGUILayout.PropertyField(repeatsProperty);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

}