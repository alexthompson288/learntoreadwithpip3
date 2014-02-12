using UnityEngine;
using System.Collections;
using UnityEditor;

namespace WingroveAudio
{
    [CustomEditor(typeof(WingroveMixBus))]
    public class WingroveMixBusEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            SerializedProperty volumeProperty = serializedObject.FindProperty("m_volumeMult");
            EditorGUILayout.HelpBox("Mixer levels can also be edited from the mixer window", MessageType.Info, true);
            GUILayout.BeginHorizontal("box");

            volumeProperty.floatValue = EditorUtilities.DBSlider("Gain", volumeProperty.floatValue);

            GUILayout.BeginVertical(GUILayout.Width(100));


            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            SerializedProperty priorityProperty = serializedObject.FindProperty("m_importance");
            priorityProperty.intValue = EditorGUILayout.IntField("Priority:", priorityProperty.intValue);

            serializedObject.ApplyModifiedProperties();
        }
    }

}