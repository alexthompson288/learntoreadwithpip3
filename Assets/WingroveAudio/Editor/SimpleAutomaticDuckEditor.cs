using UnityEngine;
using System.Collections;
using UnityEditor;
namespace WingroveAudio
{
    [CustomEditor(typeof(SimpleAutomaticDuck))]
    public class SimpleAutomaticDuckEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty groupProp = serializedObject.FindProperty("m_groupToMonitor");
            if (groupProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No group to monitor selected!", MessageType.Error, true);
            }
            EditorGUILayout.PropertyField(groupProp);

            SerializedProperty volumeProperty = serializedObject.FindProperty("m_duckingMixAmount");
            GUILayout.BeginHorizontal("box");

            volumeProperty.floatValue = EditorUtilities.DBSlider("Gain", volumeProperty.floatValue);

            GUILayout.BeginVertical(GUILayout.Width(100));

            SerializedProperty attackProp = serializedObject.FindProperty("m_attack");
            attackProp.floatValue = EditorGUILayout.FloatField("Attack", attackProp.floatValue);
            SerializedProperty releaseProp = serializedObject.FindProperty("m_release");
            releaseProp.floatValue = EditorGUILayout.FloatField("Release", releaseProp.floatValue);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

        }
    }

}