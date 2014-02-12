using UnityEngine;
using System.Collections;
using UnityEditor;
namespace WingroveAudio
{
    [CustomEditor(typeof(DuckOnEvent))]
    public class DuckOnEventEditor : Editor
    {
        int m_startBankIndex = -1;
        int m_endBankIndex = -1;
        void ShowEvent(string eventStringPropertyName, string funcName, ref int bankIndex)
        {

            SerializedProperty eventProperty = serializedObject.FindProperty(eventStringPropertyName);
            GUILayout.BeginVertical("box");
            GUILayout.Label(funcName + " event");
            bankIndex = EditorUtilities.EventProperty(bankIndex, eventProperty);
            GUILayout.EndVertical();
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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

            ShowEvent("m_startDuckEvent", "Activate duck", ref m_startBankIndex);
            ShowEvent("m_endDuckEvent", "Deactivate duck", ref m_endBankIndex);

            serializedObject.ApplyModifiedProperties();

        }
    }

}